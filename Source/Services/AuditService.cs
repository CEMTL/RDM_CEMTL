using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Solution.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solution.Services
{
    public class AuditService
    {
        private readonly HopitalMedecinsDbEntities _db;
        private readonly HttpContext _httpContext;

        public AuditService(HopitalMedecinsDbEntities db, HttpContext httpContext)
        {
            _db = db;
            _httpContext = httpContext;
        }

        public void Enregistrer(
            string tableName,
            int recordId,
            string action,
            object ancienneValeur = null,
            object nouvelleValeur = null)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            };

            string oldJson = ancienneValeur != null
                ? JsonConvert.SerializeObject(ancienneValeur, settings)
                : null;

            string newJson = nouvelleValeur != null
                ? JsonConvert.SerializeObject(nouvelleValeur, settings)
                : null;

            string changedFields = null;

            if (string.Equals(action, "UPDATE", StringComparison.OrdinalIgnoreCase)
                && ancienneValeur != null
                && nouvelleValeur != null)
            {
                changedFields = GetChangedFields(ancienneValeur, nouvelleValeur, settings);
            }

            string nomUtilisateur = _httpContext?.User?.Identity?.Name;

            // Le schéma RDM_CEMTL actuel ne contient pas de table dbo.UtilisateurMetier.
            // AuditLog.UtilisateurId est nullable et les utilisateurs applicatifs sont
            // gérés par ASP.NET Identity avec un identifiant texte. On conserve donc
            // l'identité Windows/AD dans NomUtilisateur et on laisse UtilisateurId à NULL.
            // Cela évite qu'un enregistrement métier échoue uniquement à cause de l'audit.
            int? utilisateurId = null;

            var log = new AuditLog
            {
                TableName = tableName,
                RecordId = recordId,
                Action = action,
                OldValues = oldJson,
                NewValues = newJson,
                ChangedFields = changedFields,
                UtilisateurId = utilisateurId,
                NomUtilisateur = nomUtilisateur,
                DateAction = DateTime.Now,
                AdresseIP = GetClientIpAddress(),
                UserAgent = Truncate(_httpContext?.Request?.Headers.UserAgent.ToString(), 500)
            };

            _db.AuditLog.Add(log);
        }

        private static string GetChangedFields(object avant, object apres, JsonSerializerSettings settings)
        {
            var dictAvant = ToFlatDictionary(avant, settings);
            var dictApres = ToFlatDictionary(apres, settings);

            var keys = dictAvant.Keys.Union(dictApres.Keys).Distinct().ToList();
            var changes = new Dictionary<string, object>();

            foreach (var key in keys)
            {
                var vAvant = dictAvant.ContainsKey(key) ? dictAvant[key] : null;
                var vApres = dictApres.ContainsKey(key) ? dictApres[key] : null;

                string sAvant = vAvant?.ToString();
                string sApres = vApres?.ToString();

                if (!string.Equals(sAvant, sApres, StringComparison.Ordinal))
                {
                    changes[key] = new
                    {
                        avant = sAvant,
                        apres = sApres
                    };
                }
            }

            return changes.Any()
                ? JsonConvert.SerializeObject(changes, settings)
                : null;
        }

        private static Dictionary<string, object> ToFlatDictionary(object source, JsonSerializerSettings settings)
        {
            if (source == null)
                return new Dictionary<string, object>();

            var json = JsonConvert.SerializeObject(source, settings);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
                   ?? new Dictionary<string, object>();
        }

        private string GetClientIpAddress()
        {
            try
            {
                // Ne jamais lire X-Forwarded-For directement : cet en-tête peut être
                // fourni par le client et fausser la piste d'audit. IIS/ASP.NET Core
                // normalise l'adresse du client dans Connection.RemoteIpAddress.
                return _httpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }
    }
}
