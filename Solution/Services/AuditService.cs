using Newtonsoft.Json;
using Solution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Solution.Services
{
    public class AuditService
    {
        private readonly HopitalMedecinsDbEntities _db;
        private readonly HttpContextBase _httpContext;

        public AuditService(HopitalMedecinsDbEntities db, HttpContextBase httpContext)
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

            var nomUtilisateur = _httpContext?.User?.Identity?.Name;
            int? utilisateurId = null;

            if (!string.IsNullOrWhiteSpace(nomUtilisateur))
            {
                utilisateurId = _db.UtilisateurMetier
                    .Where(u => u.Connexion == nomUtilisateur)
                    .Select(u => (int?)u.UtilisateurMetierId)
                    .FirstOrDefault();
            }

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
                UserAgent = Truncate(_httpContext?.Request?.UserAgent, 1000)
            };

            _db.AuditLog.Add(log);
        }

        private string GetChangedFields(object avant, object apres, JsonSerializerSettings settings)
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

        private Dictionary<string, object> ToFlatDictionary(object source, JsonSerializerSettings settings)
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
                var request = _httpContext?.Request;
                if (request == null)
                    return null;

                var forwarded = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrWhiteSpace(forwarded))
                {
                    var firstIp = forwarded.Split(',').FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(firstIp))
                        return firstIp.Trim();
                }

                return request.UserHostAddress;
            }
            catch
            {
                return null;
            }
        }

        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength);
        }
    }
}