using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowPDSAdiuto : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }

        private int tipoLicenza { get; set; } // 0 -> niente, 1 -> azi, 2 -> azi collegata

        private List<string> ShowMethods(Type type)
        {
            List<string> methods = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
            }

            return methods;
        }


        public WorkflowPDSAdiuto(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowPDSAdiuto));

            this.tipoLicenza = tipoLicenza;

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }

            this.tipoLicenza = tipoLicenza;
        }

        private void _AddActivity_TipoLicenza(Workflow wf)  //A.Lucchi
        {
            Activity a = wf.CreateActivity("lic");
            a.Title = "Che tipo di licenza desideri attivare?";
            a.TestoRiepilogo = "Tipo di licenza da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
            {
                    new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(15).ToShortDateString()),
                    new InputItem("standard","Standard")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("sceltaTipoModulo");
        }

        private void _AddActivity_TipoModulo(Workflow wf)
        {
            Activity a = wf.CreateActivity("sceltaTipoModulo");
            a.Title = "Quale tipo di modulo vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("nuovo", "Nuova attivazione"),
                new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = null;

            switch (tipoLicenza)
            {
                case 0:
                    b1 = a.CreateBranchTo("sceltaTipoCliente");
                    b1.Condition.IfOutputContainsItem("nuovo");
                    break;
                case 1:
                    b1 = a.CreateBranchTo("sceltaTipoAttivazione");
                    b1.Condition.IfOutputContainsItem("nuovo");
                    break;
                case 2:
                    b1 = a.CreateBranchTo("sceltaTipoAttivazioneAziColl");
                    b1.Condition.IfOutputContainsItem("nuovo");
                    break;
            }

            switch (tipoLicenza)
            {
                case 1:
                    b1 = a.CreateBranchTo("upgrade");
                    b1.Condition.IfOutputContainsItem("upgrade");
                    break;
                case 2:
                    b1 = a.CreateBranchTo("upgradeAziCollegate");
                    b1.Condition.IfOutputContainsItem("upgrade");
                    break;
            }
        }

        private void _AddActivity_Soggetto(Workflow wf)
        {
            Activity a = wf.CreateActivity("sceltaTipoCliente");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("titolare", "Azienda capogruppo e autonoma", "AD.PDS.AZI"),
                new InputItem("collegata", "Azienda in delega (sub azienda)", "AD.PDS.AZI.DEL"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("sceltaTipoAttivazione");
            b1.Condition.IfOutputContainsItem("titolare");

            Branch b2 = a.CreateBranchTo("sceltaTipoAttivazioneAziColl");
            b2.Condition.IfOutputContainsItem("collegata");
        }

        private void _AddActivity_TipoAttivazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("sceltaTipoAttivazione");
            a.Title = "Vuoi attivare la fatturazione B2B e PA o la Conservazione Documentale?";
            a.TestoRiepilogo = "Tipo di attivazione:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("FattB2BPA", "Fatturazione B2B e PA"),
                new InputItem("ConsB2BPA", "Conservazione documentale"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("azi");
            b1.Condition.IfOutputContainsItem("FattB2BPA");

            Branch b2 = a.CreateBranchTo("aziConsDOC");
            b2.Condition.IfOutputContainsItem("ConsB2BPA");
        }

        private void _AddActivity_AziendaTitolare(Workflow wf)
        {
            Activity a = wf.CreateActivity("azi");
            a.Title = "Attivazione B2B e PA, a canone o a consumo?";
            a.TestoRiepilogo = "Tipo di fatturazione:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("AD.FATT", "AD.FATT - Portale dei Servizi con Fatturazione B2B+PA a consumo", "AD.FATT"),
                new InputItem("AD.FATT.CA", "AD.FATT.CA - Portale dei Servizi con Fatturazione B2B+PA a canone", "AD.FATT.CA"),
                new InputItem("AD.FATT.CS", "AD.FATT.CS - Portale dei Servizi con Fatturazione B2B+PA+Conservazione a consumo", "AD.FATT.CS;AD.CS.TECH"),
                new InputItem("AD.FATT.CS.CA", "AD.FATT.CS.CA - Portale dei Servizi con Fatturazione B2B+PA+Conservazione a canone", "AD.FATT.CS.CA;AD.CS.TECH"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("qtaAziTitolare");
        }

        private void _AddActivity_AziendaTitolareConsDOC(Workflow wf)
        {
            Activity a = wf.CreateActivity("aziConsDOC");
            a.Title = "Conservazione documenti, a canone o a consumo?";
            a.TestoRiepilogo = "Tipo di fatturazione:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("AD.CS", "AD.CS - Conservazione sostitutiva Adiuto a consumo", "AD.CS;AD.CS.TECH"),
                new InputItem("AD.CS.CA", "AD.CS.CA - Conservazione sostitutiva Adiuto a canone", "AD.CS.CA;AD.CS.TECH"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("qtaAziTitolareConsDOC");
        }


        private void _AddActivity_QtaAziTitolare(Workflow wf)
        {
            Activity a = wf.CreateActivity("qtaAziTitolare");
            a.Title = "Quante fatture devono essere disponibili?";
            a.TestoRiepilogo = "Numero di fatture da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'adiutoQtaAziTitolare','Text':'Numero','DataType':'integer', 'MinValue':1, 'Tag':'adiutoQtaAziTitolare'}"),
            }));

            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("checkAttivazioneCS");
            Branch b1 = a.CreateBranchTo("checkMigrazione");
            //Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_CheckMigrazione(Workflow wf)
		{
			Activity a = wf.CreateActivity("checkMigrazione");
			a.Title = "Si tratta di una migrazione da IFIN/INTESA a BLUENEXT?";
			a.TestoRiepilogo = "E' una migrazione da IFIN/INTESA a BLUENEXT:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("Si", "Si", "6008319;AD.6518991"),
				new InputItem("No", "No","AD.6518991"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
			b1.Condition.IfOutputContainsItem("Si");

			Branch b2 = a.CreateBranchTo("checkRecuperoFattureAdE");
			b2.Condition.IfOutputContainsItem("No");
		}

		private void _AddActivity_CheckRecuperoFattureAdE(Workflow wf)
		{
			Activity a = wf.CreateActivity("checkRecuperoFattureAdE");
			a.Title = "Si desidera inserire il modulo Recupero Fatture da AdE?";
			a.TestoRiepilogo = "Inserire il modulo Recupero Fatture da AdE:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("Si", "Si", "6008319"),
				new InputItem("No", "No"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_QtaAziTitolareConsDOC(Workflow wf)
        {
            Activity a = wf.CreateActivity("qtaAziTitolareConsDOC");
            a.Title = "Quanti GB devono essere disponibili?";
            a.TestoRiepilogo = "Numero di fatture da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'adiutoQtaAziTitolareConsDOC','Text':'Numero','DataType':'integer', 'MinValue':1, 'Tag':'adiutoQtaAziTitolareConsDOC'}"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_CheckAttivazioneCSTitolare(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("checkAttivazioneCS");
        //    a.Title = "Attivare anche conservazione B2B/PA?";
        //    a.TestoRiepilogo = "Attivare conservazione B2B/PA";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("Si", "Si"),
        //        new InputItem("No", "No"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        private void _AddActivity_Upgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipo di moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("AD.31309", "AD.31309 - Servizio Responsabile della Conservazione e Manuale per le installazioni MONO PARTITA IVA", "AD.31309"),
                new InputItem("AD.31310", "AD.31310 - Servizio Responsabile della Conservazione e Manuale per le installazioni MULTI PARTITA IVA", "AD.31310"),
                //new InputItem("AD.CS.TECH", "AD.CS.TECH - Conservazione fatture B2B + PA per aziende capogruppo/autonome", "AD.CS.TECH"),
                new InputItem("AD.FATT.CS", "AD.FATT.CS - Portale dei Servizi con Fatturazione B2B+PA+CS a consumo", "AD.FATT.CS;AD.CS.TECH"),
                new InputItem("AD.FATT.CS.CA", "AD.FATT.CS.CA - Portale dei Servizi con Fatturazione B2B+PA+CS a canone", "AD.FATT.CS.CA;AD.CS.TECH"),
                new InputItem("AD.CS", "AD.CS - Conservazione documentale Adiuto a consumo per aziende capogruppo/autonome ", "AD.CS;AD.CS.TECH"),
                new InputItem("AD.CS.CA", "AD.CS.CA - Conservazione documentale Adiuto a canone per aziende capogruppo/autonome", "AD.CS.CA;AD.CS.TECH"),
				new InputItem("AD.6518991", "AD.6518991 - Mantenimento in linea Fatture per Aziende", "AD.6518991"),
				new InputItem("6008319", "6008319 - Recupero automatico fatture da portale Fatture&Corrispettivi", "6008319"),
				new InputItem("AD.ORDINIPA", "AD.ORDINIPA - Attivazione ordini NSO e PEPPOL", "AD.ORDINIPA"),
			}));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("qtaAziTitolareConsDOC");
            b1.Condition.IfOutputContainsItem("AD.CS");

            Branch b2 = a.CreateBranchTo("qtaAziTitolareConsDOC");
            b2.Condition.IfOutputContainsItem("AD.CS.CA");

            Branch b3 = a.CreateBranchTo("qtaAziTitolare");
            b3.Condition.IfOutputContainsItem("AD.FATT.CS");

            Branch b4 = a.CreateBranchTo("qtaAziTitolare");
            b4.Condition.IfOutputContainsItem("AD.FATT.CA.CS");

            Branch b5 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UpgradeAziCollegate(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradeAziCollegate");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipo di moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                //new InputItem("AD.31309", "AD.31309 - Servizio Responsabile della Conservazione e Manuale per le installazioni MONO PARTITA IVA", "AD.31309"),
                //new InputItem("AD.31310", "AD.31310 - Servizio Responsabile della Conservazione e Manuale per le installazioni MULTI PARTITA IVA", "AD.31310"),
                //new InputItem("AD.DEL.CS", "AD.DEL.CS - Conservazione fatture B2B+PA per aziende in delega (subaziende)", "AD.DEL.CS"),
                new InputItem("AD.CS.TECH.DEL", "AD.CS.TECH.DEL - Conservazione fatture B2B+PA per aziende in delega (subaziende)", "AD.CS.TECH.DEL"),
                new InputItem("AD.CS.DEL", "AD.CS.DEL - Conservazione documentale Adiuto per aziende in delega (subaziende)", "AD.CS.DEL"),
				new InputItem("AD.ORDINIPA", "AD.ORDINIPA - Attivazione ordini NSO e PEPPOL", "AD.ORDINIPA"),
			}));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_CanoneAziTitolare(Workflow wf)
        {
            Activity a = wf.CreateActivity("canoneAziTitolare");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipo di moduli da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("AD.FATT.CA", "AD.FATT.CA - Portale dei Servizi con Fatturazione B2B+PA a canone", "AD.FATT.CA"),
                new InputItem("AD.FATT.CS.CA", "AD.FATT.CS.CA - Portale dei Servizi con Fatturazione B2B+PA a canone", "AD.FATT.CS.CA"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_AziCollegata(Workflow wf)
        {
            Activity a = wf.CreateActivity("aziColl");
            a.Title = "Attivare conservazione B2B/PA in delega?";
            a.TestoRiepilogo = "Attivare conservazione B2B/PA in delega";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("Si", "Si", "AD.FATT.DEL;AD.CS.TECH.DEL"),
                new InputItem("No", "No", "AD.FATT.DEL"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("aziCollPITitolare");
        }

        private void _AddActivity_TipoAttivazioneAziColl(Workflow wf)
        {
            Activity a = wf.CreateActivity("sceltaTipoAttivazioneAziColl");
            a.Title = "Vuoi attivare la fatturazione B2B e PA o la Conservazione Documentale?";
            a.TestoRiepilogo = "Tipo di attivazione:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("FattB2BPAAziColl", "Fatturazione B2B e PA"),
                new InputItem("ConsB2BPAAziColl", "Conservazione documentale", "AD.CS.DEL;AD.CS.TECH.DEL"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("aziColl");
            b1.Condition.IfOutputContainsItem("FattB2BPAAziColl");

            Branch b2 = a.CreateBranchTo("aziCollPITitolare");
            b2.Condition.IfOutputContainsItem("ConsB2BPAAziColl");
        }

        private void _AddActivity_AziCollegataPITitolare(Workflow wf)
        {
            Activity a = wf.CreateActivity("aziCollPITitolare");
            a.Title = "Partita iva del titolare della licenza";
            a.TestoRiepilogo = "Partita iva del titolare della licenza";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'PITitAziCollAd','Text':'PI titolare','DataType':'string'}"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_UploadPDF(Workflow wf)
        {
            Activity a = wf.CreateActivity("uploadFile");
            a.Title = "Carica il pdf del contratto";
            a.TestoRiepilogo = "PDF del contratto:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'uploadFile','Text':'Carica PDF Delega Invio Firma','DataType':'blob', 'Tag':'Blob'}"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchToSummary();
        }


        private void _AddActivity_Summary(Workflow wf)
        {
            Activity a = wf.CreateSummaryActivity();
            a.Title = "Vuoi procedere con l'attivazione?";
            a.DrawPage = _DrawPage;
        }

        private void _AddActivity_Outcome(Workflow wf)
        {
            Activity a = wf.CreateOutcomeActivity();
            a.DrawPage = _DrawPage;
        }
    }
}