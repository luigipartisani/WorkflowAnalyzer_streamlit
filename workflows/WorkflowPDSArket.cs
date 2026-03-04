using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowPDSArket : Workflow
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


        public WorkflowPDSArket(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowPDSArket));

            this.tipoLicenza = tipoLicenza;

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }

            this.tipoLicenza = tipoLicenza;
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
					b1 = a.CreateBranchTo("upgrade");
					b1.Condition.IfOutputContainsItem("upgrade");
                    break;
				case 2:
                    b1 = a.CreateBranchTo("sceltaTipoAttivazioneAziColl");
                    b1.Condition.IfOutputContainsItem("nuovo");
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
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("FattB2BPA", "Fatturazione e Conservazione B2B/PA","AD.FATT.CS.CA;AD.31309;AD.CS.TECH;6008319;AD.6518991"),
				new InputItem("ConsB2BPA", "Conservazione documentale","AD.CS.TECH;AD.31309;AD.CS.CA;AD.31300BN"),
			}));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("qtaAziTitolare");
            b1.Condition.IfOutputContainsItem("FattB2BPA");

            Branch b2 = a.CreateBranchTo("qtaAziTitolareConsDOC");
            b2.Condition.IfOutputContainsItem("ConsB2BPA");
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
			a.AllowNoChoice = true;
            Branch b1 = a.CreateBranchTo("checkAziAggiuntiveConsDOC");
        }

		private void _AddActivity_QtaAziAggiuntiveConsDOC(Workflow wf)
		{
			Activity a = wf.CreateActivity("checkAziAggiuntiveConsDOC");
			a.Title = "Sono presenti aziende aggiuntive a cui attivare il servizio?<br><span style='font-size:12px'>Se non presenti, non compilare il campo e cliccare su AVANTI</span>";
			a.TestoRiepilogo = "Numero di aziende aggiuntive da attivare:";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				 new InputItem("{'Key':'adiutoQtaAziAggiuntiveConsDOC','Text':'Numero','DataType':'integer', 'MinValue':1, 'Tag':'adiutoQtaAziAggiuntiveConsDOC'}"),
			}));

			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_Upgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgrade");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipo di moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("AD.FATT.CS.CA", "AD.FATT.CS.CA - Fatturazione B2B/PA", "AD.31309;6008319;AD.6518991"),
                new InputItem("AD.CS.CA", "AD.CS.CA - Conservazione documentale a canone", "AD.31300BN"),
			}));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("qtaAziTitolareConsDOC");
            b1.Condition.IfOutputContainsItem("AD.CS.CA");

            Branch b2 = a.CreateBranchTo("qtaAziTitolare");
            b2.Condition.IfOutputContainsItem("AD.FATT.CS.CA");
        }

        private void _AddActivity_UpgradeAziCollegate(Workflow wf)
        {
            Activity a = wf.CreateActivity("upgradeAziCollegate");
            a.Title = "Quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipo di moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				new InputItem("AD.FATT.DEL", "AD.FATT.DEL - Fatturazione B2B/PA in delega", "AD.FATT.DEL"),
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

        private void _AddActivity_TipoAttivazioneAziColl(Workflow wf)
        {
            Activity a = wf.CreateActivity("sceltaTipoAttivazioneAziColl");
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("FattB2BPAAziColl", "Fatturazione e Conservazione B2B/PA","AD.FATT.DEL;AD.CS.TECH.DEL"),
                new InputItem("ConsB2BPAAziColl", "Conservazione documentale", "AD.CS.TECH.DEL"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("aziCollPITitolare");
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