using Microsoft.EntityFrameworkCore;

namespace Solution.Models;

public partial class HopitalMedecinsDbEntities : DbContext
{
    public HopitalMedecinsDbEntities(DbContextOptions<HopitalMedecinsDbEntities> options) : base(options) { }

    public virtual DbSet<AuditLog> AuditLog { get; set; }
    public virtual DbSet<AffectationOrganisationnelle> AffectationOrganisationnelle { get; set; }
    public virtual DbSet<CategoriePratique> CategoriePratique { get; set; }
    public virtual DbSet<CodeCotisation> CodeCotisation { get; set; }
    public virtual DbSet<Comite> Comite { get; set; }
    public virtual DbSet<CotisationAnnuelle> CotisationAnnuelle { get; set; }
    public virtual DbSet<Departement> Departement { get; set; }
    public virtual DbSet<EntenteFDC> EntenteFDC { get; set; }
    public virtual DbSet<FitTest> FitTest { get; set; }
    public virtual DbSet<Genre> Genre { get; set; }
    public virtual DbSet<Installation> Installation { get; set; }
    public virtual DbSet<InstallationService> InstallationService { get; set; }
    public virtual DbSet<InstanceDecision> InstanceDecision { get; set; }
    public virtual DbSet<JournalImport> JournalImport { get; set; }
    public virtual DbSet<Medecin> Medecin { get; set; }
    public virtual DbSet<MedecinAdresse> MedecinAdresse { get; set; }
    public virtual DbSet<MedecinAffectationClinique> MedecinAffectationClinique { get; set; }
    public virtual DbSet<DocumentMedecin> DocumentMedecin { get; set; }
    public virtual DbSet<MedecinComite> MedecinComite { get; set; }
    public virtual DbSet<MedecinCourriel> MedecinCourriel { get; set; }
    public virtual DbSet<MedecinInstallation> MedecinInstallation { get; set; }
    public virtual DbSet<MedecinPrivilege> MedecinPrivilege { get; set; }
    public virtual DbSet<MedecinTelephone> MedecinTelephone { get; set; }
    public virtual DbSet<Nomination> Nomination { get; set; }
    public virtual DbSet<NominationAdministrative> NominationAdministrative { get; set; }
    public virtual DbSet<PrivilegeReference> PrivilegeReference { get; set; }
    public virtual DbSet<Service> Service { get; set; }
    public virtual DbSet<StatutMedecin> StatutMedecin { get; set; }
    public virtual DbSet<TitreMedecin> TitreMedecin { get; set; }
    public virtual DbSet<TypeInstanceDecision> TypeInstanceDecision { get; set; }
    public virtual DbSet<TypeNomination> TypeNomination { get; set; }
    public virtual DbSet<TypePaiement> TypePaiement { get; set; }
    public virtual DbSet<TypePermis> TypePermis { get; set; }
    public virtual DbSet<TypeCourriel> TypeCourriel { get; set; }
    public virtual DbSet<vw_CotisationsParAnnee> vw_CotisationsParAnnee { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLog", "dbo");
            entity.HasKey(e => e.AuditLogId);
            entity.Property(e => e.AuditLogId).ValueGeneratedOnAdd();
            entity.Property(e => e.TableName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(10).IsRequired();
            entity.Property(e => e.NomUtilisateur).HasMaxLength(255);
            entity.Property(e => e.AdresseIP).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });

        modelBuilder.Entity<AffectationOrganisationnelle>(entity =>
        {
            entity.ToTable("AffectationOrganisationnelle", "dbo");
            entity.HasKey(e => e.AffectationOrganisationnelleId);
            entity.Property(e => e.AffectationOrganisationnelleId).ValueGeneratedOnAdd();
            entity.Property(e => e.RoleCode).HasMaxLength(40).IsUnicode(false).IsRequired();
            entity.Property(e => e.NomAffiche).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Titre).HasMaxLength(300);
            entity.Property(e => e.Courriel).HasMaxLength(255);
            entity.Property(e => e.CourrielSource).HasMaxLength(255);
            entity.Property(e => e.Telephone).HasMaxLength(50);
            entity.Property(e => e.PosteTelephone).HasMaxLength(20);
            entity.Property(e => e.SiteNomSource).HasMaxLength(200);
            entity.Property(e => e.RegroupementSource).HasMaxLength(200);
            entity.Property(e => e.DateDebut).HasColumnType("date");
            entity.Property(e => e.DateFin).HasColumnType("date");
            entity.Property(e => e.SourceDonnees).HasMaxLength(200);
            entity.Property(e => e.DateCreation).HasPrecision(0);
            entity.Property(e => e.DateModification).HasPrecision(0);
            entity.HasIndex(e => new { e.SourceDonnees, e.SourceLigne, e.RoleCode })
                .IsUnique()
                .HasFilter("[SourceDonnees] IS NOT NULL AND [SourceLigne] IS NOT NULL");
            entity.HasIndex(e => new { e.DepartementId, e.RoleCode, e.EstActive, e.DateDebut });
            entity.HasIndex(e => new { e.ServiceId, e.RoleCode, e.EstActive, e.DateDebut });
        });

        modelBuilder.Entity<CategoriePratique>(entity =>
        {
            entity.ToTable("CategoriePratique", "ref");
            entity.HasKey(e => e.CategoriePratiqueId);
            entity.Property(e => e.CategoriePratiqueId).ValueGeneratedOnAdd();
            entity.Property(e => e.Libelle).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<CodeCotisation>(entity =>
        {
            entity.ToTable("CodeCotisation", "dbo");
            entity.HasKey(e => e.CodeCotisationId);
            entity.Property(e => e.CodeCotisationId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodeCotisation1).HasColumnName("CodeCotisation").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Comite>(entity =>
        {
            entity.ToTable("Comite", "dbo");
            entity.HasKey(e => e.ComiteId);
            entity.Property(e => e.ComiteId).ValueGeneratedOnAdd();
            entity.Property(e => e.NomComite).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<CotisationAnnuelle>(entity =>
        {
            entity.ToTable("CotisationAnnuelle", "dbo");
            entity.HasKey(e => e.CotisationAnnuelleId);
            entity.Property(e => e.CotisationAnnuelleId).ValueGeneratedOnAdd();
            entity.Property(e => e.DescriptionSource).HasMaxLength(255);
            entity.Property(e => e.MontantInitial).HasPrecision(12, 2);
            entity.Property(e => e.MotifReduction).HasMaxLength(255);
            entity.Property(e => e.MontantReduction).HasPrecision(12, 2);
            entity.Property(e => e.PenaliteTexte).HasMaxLength(100);
            entity.Property(e => e.MontantPenalite).HasPrecision(12, 2);
            entity.Property(e => e.MontantFinal).HasPrecision(12, 2);
            entity.Property(e => e.EtatCotisation).HasMaxLength(100);
            entity.Property(e => e.MontantPaye).HasPrecision(12, 2);
            entity.Property(e => e.TypePaiementTexte).HasMaxLength(100);
            entity.Property(e => e.NoDepot).HasMaxLength(50);
            entity.Property(e => e.RecuImposition).HasMaxLength(50);
        });


        modelBuilder.Entity<MedecinAffectationClinique>(entity =>
        {
            entity.ToTable("MedecinAffectationClinique", "dbo");
            entity.HasKey(e => e.MedecinAffectationCliniqueId);
            entity.Property(e => e.MedecinAffectationCliniqueId).ValueGeneratedOnAdd();
            entity.Property(e => e.TypeAffectation).HasMaxLength(100);
            entity.Property(e => e.DateDebut).HasColumnType("date");
            entity.Property(e => e.DateFin).HasColumnType("date");
            entity.Property(e => e.Commentaire).HasMaxLength(500);
            entity.Property(e => e.SourceDonnees).HasMaxLength(200);
            entity.Property(e => e.DateCreation).HasPrecision(0);
            entity.Property(e => e.DateModification).HasPrecision(0);
            entity.HasIndex(e => new { e.MedecinId, e.DepartementId, e.ServiceId, e.InstallationId, e.DateDebut });
            entity.HasIndex(e => new { e.MedecinId, e.EstPrincipale, e.EstActive });
        });

        modelBuilder.Entity<DocumentMedecin>(entity =>
        {
            entity.ToTable("DocumentMedecin", "dbo");
            entity.HasKey(e => e.DocumentMedecinId);
            entity.Property(e => e.DocumentMedecinId).ValueGeneratedOnAdd();
            entity.Property(e => e.TypeDocument).HasMaxLength(100).IsRequired();
            entity.Property(e => e.NomFichier).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.HashSHA256).HasMaxLength(64).IsUnicode(false).IsRequired();
            entity.Property(e => e.DateDocument).HasColumnType("date");
            entity.Property(e => e.DateExpiration).HasColumnType("date");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EntiteLiee).HasMaxLength(50);
            entity.Property(e => e.DeposePar).HasMaxLength(256);
            entity.Property(e => e.Contenu).IsRequired();
            entity.Property(e => e.DateDepot).HasPrecision(0);
            entity.HasIndex(e => new { e.MedecinId, e.EstActif, e.DateExpiration });
            entity.HasIndex(e => e.HashSHA256);
        });

        modelBuilder.Entity<Departement>(entity =>
        {
            entity.ToTable("Departement", "dbo");
            entity.HasKey(e => e.DepartementId);
            entity.Property(e => e.DepartementId).ValueGeneratedOnAdd();
            entity.Property(e => e.NomDepartement).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CodeDepartement).HasMaxLength(50);
            entity.HasIndex(e => e.NomDepartement).IsUnique();
        });

        modelBuilder.Entity<EntenteFDC>(entity =>
        {
            entity.ToTable("EntenteFDC", "dbo");
            entity.HasKey(e => e.EntenteFDCId);
            entity.Property(e => e.EntenteFDCId).ValueGeneratedOnAdd();
            entity.Property(e => e.TypeEntenteFDC).HasMaxLength(150);
            entity.Property(e => e.PEMVise).HasMaxLength(150);
        });

        modelBuilder.Entity<FitTest>(entity =>
        {
            entity.ToTable("FitTest", "dbo");
            entity.HasKey(e => e.FitTestId);
            entity.Property(e => e.FitTestId).ValueGeneratedOnAdd();
            entity.Property(e => e.Resultat).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Commentaire).HasMaxLength(500);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.ToTable("Genre", "ref");
            entity.HasKey(e => e.GenreId);
            entity.Property(e => e.GenreId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodeGenre).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Libelle).HasMaxLength(50);
        });

        modelBuilder.Entity<Installation>(entity =>
        {
            entity.ToTable("Installation", "dbo");
            entity.HasKey(e => e.InstallationId);
            entity.Property(e => e.InstallationId).ValueGeneratedOnAdd();
            entity.Property(e => e.NomInstallation).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CodeInstallation).HasMaxLength(50);
        });

        modelBuilder.Entity<InstallationService>(entity =>
        {
            entity.ToTable("InstallationService", "dbo");
            entity.HasKey(e => e.InstallationServiceId);
            entity.Property(e => e.InstallationServiceId).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.InstallationId);
            entity.HasIndex(e => e.ServiceId);
            entity.HasIndex(e => new { e.InstallationId, e.ServiceId })
                .IsUnique()
                .HasFilter("[EstActif] = 1");
        });

        modelBuilder.Entity<InstanceDecision>(entity =>
        {
            entity.ToTable("InstanceDecision", "dbo");
            entity.HasKey(e => e.InstanceDecisionId);
            entity.Property(e => e.InstanceDecisionId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodeInstance).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LibelleInstance).HasMaxLength(255);
        });

        modelBuilder.Entity<JournalImport>(entity =>
        {
            entity.ToTable("JournalImport", "dbo");
            entity.HasKey(e => e.JournalImportId);
            entity.Property(e => e.JournalImportId).ValueGeneratedOnAdd();
            entity.Property(e => e.NomFichier).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Commentaire).HasMaxLength(1000);
        });

        modelBuilder.Entity<Medecin>(entity =>
        {
            entity.ToTable("Medecin", "dbo");
            entity.HasKey(e => e.MedecinId);
            entity.Property(e => e.MedecinId).ValueGeneratedOnAdd();
            entity.Property(e => e.Permis).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nom).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Prenom).HasMaxLength(150).IsRequired();
            entity.Property(e => e.NAM).HasMaxLength(20);
        });

        modelBuilder.Entity<MedecinAdresse>(entity =>
        {
            entity.ToTable("MedecinAdresse", "dbo");
            entity.HasKey(e => e.MedecinAdresseId);
            entity.Property(e => e.MedecinAdresseId).ValueGeneratedOnAdd();
            entity.Property(e => e.AdresseLigne1).HasMaxLength(255);
            entity.Property(e => e.Ville).HasMaxLength(100);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.CodePostal).HasMaxLength(20);
        });

        modelBuilder.Entity<MedecinComite>(entity =>
        {
            entity.ToTable("MedecinComite", "dbo");
            entity.HasKey(e => e.MedecinComiteId);
            entity.Property(e => e.MedecinComiteId).ValueGeneratedOnAdd();
            entity.Property(e => e.RoleMembre).HasMaxLength(100);
            entity.Property(e => e.Commentaire).HasMaxLength(500);
        });

        modelBuilder.Entity<MedecinCourriel>(entity =>
        {
            entity.ToTable("MedecinCourriel", "dbo");
            entity.HasKey(e => e.MedecinCourrielId);
            entity.Property(e => e.MedecinCourrielId).ValueGeneratedOnAdd();
            entity.Property(e => e.TypeCourriel).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AdresseCourriel).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<TypeCourriel>(entity =>
        {
            entity.ToTable("TypeCourriel", "ref");
            entity.HasKey(e => e.TypeCourrielId);
            entity.Property(e => e.TypeCourrielId).ValueGeneratedOnAdd();
            entity.Property(e => e.Libelle).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Libelle).IsUnique();
        });

        modelBuilder.Entity<MedecinInstallation>(entity =>
        {
            entity.ToTable("MedecinInstallation", "dbo");
            entity.HasKey(e => e.MedecinInstallationId);
            entity.Property(e => e.MedecinInstallationId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<MedecinPrivilege>(entity =>
        {
            entity.ToTable("MedecinPrivilege", "dbo");
            entity.HasKey(e => e.MedecinPrivilegeId);
            entity.Property(e => e.MedecinPrivilegeId).ValueGeneratedOnAdd();
            entity.Property(e => e.Etat).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NatureActivites).HasMaxLength(500);
            entity.Property(e => e.ChampActivites).HasMaxLength(500);
            entity.Property(e => e.Commentaire).HasMaxLength(1000);
            entity.Property(e => e.NumeroResolutionCA).HasMaxLength(50);
        });

        modelBuilder.Entity<MedecinTelephone>(entity =>
        {
            entity.ToTable("MedecinTelephone", "dbo");
            entity.HasKey(e => e.MedecinTelephoneId);
            entity.Property(e => e.MedecinTelephoneId).ValueGeneratedOnAdd();
            entity.Property(e => e.TypeTelephone).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NumeroTelephone).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Nomination>(entity =>
        {
            entity.ToTable("Nomination", "dbo");
            entity.HasKey(e => e.NominationId);
            entity.Property(e => e.NominationId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodeNomination).HasMaxLength(30).IsRequired();
            entity.Property(e => e.LibelleNomination).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<NominationAdministrative>(entity =>
        {
            entity.ToTable("NominationAdministrative", "dbo");
            entity.HasKey(e => e.NominationAdministrativeId);
            entity.Property(e => e.NominationAdministrativeId).ValueGeneratedOnAdd();
            entity.Property(e => e.LibelleNomination).HasMaxLength(255);
        });

        modelBuilder.Entity<PrivilegeReference>(entity =>
        {
            entity.ToTable("PrivilegeReference", "dbo");
            entity.HasKey(e => e.PrivilegeReferenceId);
            entity.Property(e => e.PrivilegeReferenceId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodePrivilege).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LibellePrivilege).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Categorie).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.NecessiteService).HasColumnName("NecessiteUnite");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Service", "dbo");
            entity.HasKey(e => e.ServiceId);
            entity.Property(e => e.ServiceId).ValueGeneratedOnAdd();
            entity.Property(e => e.NomService).HasMaxLength(250).IsRequired();
            entity.Property(e => e.CodeService).HasMaxLength(50);
            entity.Property(e => e.RegroupementSource).HasMaxLength(200);
            entity.HasIndex(e => e.DepartementId);
            entity.HasIndex(e => new { e.DepartementId, e.NomService }).IsUnique();
        });

        modelBuilder.Entity<StatutMedecin>(entity =>
        {
            entity.ToTable("StatutMedecin", "ref");
            entity.HasKey(e => e.StatutMedecinId);
            entity.Property(e => e.StatutMedecinId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodeStatut).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Libelle).HasMaxLength(100);
        });

        modelBuilder.Entity<TitreMedecin>(entity =>
        {
            entity.ToTable("TitreMedecin", "ref");
            entity.HasKey(e => e.TitreId);
            entity.Property(e => e.TitreId).ValueGeneratedOnAdd();
            entity.Property(e => e.Libelle).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<TypeInstanceDecision>(entity =>
        {
            entity.ToTable("TypeInstanceDecision", "dbo");
            entity.HasKey(e => e.TypeInstanceDecisionId);
            entity.Property(e => e.TypeInstanceDecisionId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodeType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Libelle).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<TypeNomination>(entity =>
        {
            entity.ToTable("TypeNomination", "dbo");
            entity.HasKey(e => e.TypeNominationId);
            entity.Property(e => e.TypeNominationId).ValueGeneratedOnAdd();
            entity.Property(e => e.CodeTypeNomination).HasMaxLength(30).IsRequired();
            entity.Property(e => e.LibelleTypeNomination).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<TypePaiement>(entity =>
        {
            entity.ToTable("TypePaiement", "dbo");
            entity.HasKey(e => e.TypePaiementId);
            entity.Property(e => e.TypePaiementId).ValueGeneratedOnAdd();
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<TypePermis>(entity =>
        {
            entity.ToTable("TypePermis", "ref");
            entity.HasKey(e => e.TypePermisId);
            entity.Property(e => e.TypePermisId).ValueGeneratedOnAdd();
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<vw_CotisationsParAnnee>(entity =>
        {
            entity.ToView("vw_CotisationsParAnnee", "dbo");
            entity.HasKey(e => new { e.CotisationAnnuelleId, e.MedecinId, e.Permis, e.Nom, e.Prenom, e.AnneeExercice });
            entity.Property(e => e.Permis).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nom).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Prenom).HasMaxLength(150).IsRequired();
            entity.Property(e => e.CodeCotisation).HasMaxLength(50);
            entity.Property(e => e.MontantInitial).HasPrecision(12, 2);
            entity.Property(e => e.MontantFinal).HasPrecision(12, 2);
            entity.Property(e => e.MontantPaye).HasPrecision(12, 2);
            entity.Property(e => e.EtatCotisation).HasMaxLength(100);
        });

        modelBuilder.Entity<CotisationAnnuelle>()
            .HasOne(d => d.CodeCotisation)
            .WithMany(p => p.CotisationAnnuelle)
            .HasForeignKey(d => d.CodeCotisationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CotisationAnnuelle>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.CotisationAnnuelle)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CotisationAnnuelle>()
            .HasOne(d => d.TypePaiement)
            .WithMany(p => p.CotisationAnnuelle)
            .HasForeignKey(d => d.TypePaiementId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<EntenteFDC>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.EntenteFDC)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FitTest>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.FitTest)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InstanceDecision>()
            .HasOne(d => d.TypeInstanceDecision)
            .WithMany(p => p.InstanceDecision)
            .HasForeignKey(d => d.TypeInstanceDecisionId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinAdresse>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.MedecinAdresse)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinComite>()
            .HasOne(d => d.Comite)
            .WithMany(p => p.MedecinComite)
            .HasForeignKey(d => d.ComiteId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinComite>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.MedecinComite)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinCourriel>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.MedecinCourriel)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinCourriel>()
            .HasOne(d => d.TypeCourrielReference)
            .WithMany(p => p.MedecinCourriels)
            .HasForeignKey(d => d.TypeCourrielId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinInstallation>()
            .HasOne(d => d.Installation)
            .WithMany(p => p.MedecinInstallation)
            .HasForeignKey(d => d.InstallationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinInstallation>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.MedecinInstallation)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinPrivilege>()
            .HasOne(d => d.Installation)
            .WithMany(p => p.MedecinPrivilege)
            .HasForeignKey(d => d.InstallationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinPrivilege>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.MedecinPrivilege)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinPrivilege>()
            .HasOne(d => d.PrivilegeReference)
            .WithMany(p => p.MedecinPrivilege)
            .HasForeignKey(d => d.PrivilegeReferenceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinPrivilege>()
            .HasOne(d => d.Service)
            .WithMany(p => p.MedecinPrivileges)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinTelephone>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.MedecinTelephone)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Medecin>()
            .HasOne(d => d.CategoriePratique)
            .WithMany(p => p.Medecin)
            .HasForeignKey(d => d.CategoriePratiqueId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Medecin>()
            .HasOne(d => d.Genre)
            .WithMany(p => p.Medecin)
            .HasForeignKey(d => d.GenreId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Medecin>()
            .HasOne(d => d.StatutMedecin)
            .WithMany(p => p.Medecin)
            .HasForeignKey(d => d.StatutMedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Medecin>()
            .HasOne(d => d.TitreMedecin)
            .WithMany(p => p.Medecin)
            .HasForeignKey(d => d.TitreId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Medecin>()
            .HasOne(d => d.TypePermis)
            .WithMany(p => p.Medecin)
            .HasForeignKey(d => d.TypePermisId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<NominationAdministrative>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.NominationAdministrative)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<NominationAdministrative>()
            .HasOne(d => d.Nomination)
            .WithMany(p => p.NominationAdministrative)
            .HasForeignKey(d => d.NominationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<NominationAdministrative>()
            .HasOne(d => d.TypeNomination)
            .WithMany(p => p.NominationAdministrative)
            .HasForeignKey(d => d.TypeNominationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InstallationService>()
            .HasOne(d => d.Installation)
            .WithMany(p => p.InstallationService)
            .HasForeignKey(d => d.InstallationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AffectationOrganisationnelle>()
            .HasOne(d => d.Departement)
            .WithMany(p => p.AffectationsOrganisationnelles)
            .HasForeignKey(d => d.DepartementId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AffectationOrganisationnelle>()
            .HasOne(d => d.Service)
            .WithMany(p => p.AffectationsOrganisationnelles)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AffectationOrganisationnelle>()
            .HasOne(d => d.Medecin)
            .WithMany()
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AffectationOrganisationnelle>()
            .HasOne(d => d.Installation)
            .WithMany()
            .HasForeignKey(d => d.InstallationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<InstallationService>()
            .HasOne(d => d.Service)
            .WithMany(p => p.InstallationServices)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);


        modelBuilder.Entity<MedecinAffectationClinique>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.MedecinAffectationClinique)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinAffectationClinique>()
            .HasOne(d => d.Departement)
            .WithMany(p => p.MedecinAffectationsCliniques)
            .HasForeignKey(d => d.DepartementId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinAffectationClinique>()
            .HasOne(d => d.Service)
            .WithMany(p => p.MedecinAffectationsCliniques)
            .HasForeignKey(d => d.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MedecinAffectationClinique>()
            .HasOne(d => d.Installation)
            .WithMany(p => p.MedecinAffectationsCliniques)
            .HasForeignKey(d => d.InstallationId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DocumentMedecin>()
            .HasOne(d => d.Medecin)
            .WithMany(p => p.DocumentMedecin)
            .HasForeignKey(d => d.MedecinId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Service>()
            .HasOne(d => d.Departement)
            .WithMany(p => p.Services)
            .HasForeignKey(d => d.DepartementId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
