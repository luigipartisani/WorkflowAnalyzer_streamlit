using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowOpenBanking : Workflow
	{
		private Action<StateContext> _DrawPage { get; set; }
		private int tipoLicenza { get; set; } // 0 -> niente, 1 -> comm, 2 -> azi, 3 -> subcliente
		private int tipoLink { get; set; } // -1 -> nulla, 0 -> comm, 1 -> sub, 3 -> azi

		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowOpenBanking(string key, string title, Action<StateContext> drawPage, int tipoLicenza, int tipoLink) : base(key, title)
		{
			_DrawPage = drawPage;

			this.tipoLicenza = tipoLicenza;
			this.tipoLink = tipoLink;

			List<string> activities = GetActivities(typeof(WorkflowOpenBanking));

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoContratto(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoContratto");
			a.Title = "Che tipo di contratto vuoi attivare?";
			//a.Description = "Breve descrizione...";
			a.TestoRiepilogo = "Tipo di contratto da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("nuovaAttivazione", "Nuova attivazione"),
			   new InputItem("upgrade", "Upgrade"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = null;
			Branch b2 = null;

			switch (tipoLicenza)  // 0 -> niente, 1 -> comm, 2 -> azi, 3 -> subcliente, 4 -> azienda collegata
			{
				case 0:

					if (tipoLink == -1)
					{
						b1 = a.CreateBranchTo("tipoSogg");
						b1.Condition.IfOutputContainsItem("nuovaAttivazione");
					}
					else if (tipoLink == 0)
					{
						b1 = a.CreateBranchTo("nuovaAttComm");
						b1.Condition.IfOutputContainsItem("nuovaAttivazione");
					}
					else if (tipoLink == 3)
					{
						b1 = a.CreateBranchTo("nuovaAttAzi");
						b1.Condition.IfOutputContainsItem("nuovaAttivazione");
					}
					else if (tipoLink == 1 || tipoLink == 3)
					{
						b1 = a.CreateBranchTo("nuovaAttSubCli");
						b1.Condition.IfOutputContainsItem("nuovaAttivazione");
					}

					b2 = a.CreateBranchTo("upgradeOpenBanking");
					b2.Condition.IfOutputContainsItem("upgrade");

					break;

				case 1:
					b1 = a.CreateBranchTo("nuovaAttComm");
					b1.Condition.IfOutputContainsItem("nuovaAttivazione");
					b2 = a.CreateBranchTo("upgradeComm");
					b2.Condition.IfOutputContainsItem("upgrade");
					break;

				case 2:
					b1 = a.CreateBranchTo("nuovaAttAzi");
					b1.Condition.IfOutputContainsItem("nuovaAttivazione");
					b2 = a.CreateBranchTo("upgradeAzi");
					b2.Condition.IfOutputContainsItem("upgrade");
					break;

				case 3:
				case 4:
					b1 = a.CreateBranchTo("nuovaAttSubCli");
					b1.Condition.IfOutputContainsItem("nuovaAttivazione");
					b2 = a.CreateBranchTo("upgradeSubCli");
					b2.Condition.IfOutputContainsItem("upgrade");
					break;
			}
		}

		private void _AddActivity_NuovaAttivazioneSenzaB2B(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoSogg");
			a.Title = "Che tipo di attivazione vuoi effettuare?";
			a.TestoRiepilogo = "Tipo di attivazione:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("commercialista", "Commercialista","PDS.COMM"),
			   new InputItem("azienda", "Azienda","PDS.AZI"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("nuovaAttComm");
			b1.Condition.IfOutputContainsItem("commercialista");

			Branch b2 = a.CreateBranchTo("nuovaAttAzi");
			b2.Condition.IfOutputContainsItem("azienda");
		}

		private void _AddActivity_NuovaAttivazioneAzienda(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttAzi");
			a.Title = "Che tipo di azienda vuoi attivare?";
			a.TestoRiepilogo = "Tipo di azienda:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("autonoma", "Azienda Autonoma"),
			   new InputItem("capogruppo", "Azienda Capogruppo"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("nuovaAttAziAutonoma");
			b1.Condition.IfOutputContainsItem("autonoma");

			Branch b2 = a.CreateBranchTo("nuovaAttAziCapogruppo");
			b2.Condition.IfOutputContainsItem("capogruppo");

		}

		private void _AddActivity_NuovaAttivazioneAziendaAuto(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttAziAutonoma");
			a.Title = "Quale modulo desideri attivare?";
			a.TestoRiepilogo = "Modulo da attivare";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("POB.001", "POB.001 - Portale Open Banking per AZIENDA con Espando Modulare o Italworking", "POB.001"),
			   new InputItem("POB.021", "POB.021 - Portale Open Banking per AZIENDA", "POB.021"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_NuovaAttivazioneAziendaCapogruppo(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttAziCapogruppo");
			a.Title = "Quale modulo desideri attivare?";
			a.TestoRiepilogo = "Modulo da attivare";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("POB.002", "POB.002 - Portale Open Banking per AZIENDA CAPOGRUPPO con Espando Modulare o Italworking","POB.002"),
			   new InputItem("POB.022", "POB.022 - Portale Open Banking per AZIENDA CAPOGRUPPO - 1 Azienda","POB.022"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_NuovaAttivazioneCommercialista(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttComm");
			a.Title = "Quale modulo desideri attivare?";
			a.TestoRiepilogo = "Modulo da attivare";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("POB.011", "POB.011 - Portale Open Banking per COMMERCIALISTA - fino a 5 Aziende","POB.011"),
			   new InputItem("POB.004", "POB.004 - Portale Open Banking per COMMERCIALISTA - fino a 10 Aziende","POB.004"),
			   new InputItem("POB.005", "POB.005 - Portale Open Banking per COMMERCIALISTA - fino a 25 Aziende","POB.005"),
			   new InputItem("POB.006", "POB.006 - Portale Open Banking per COMMERCIALISTA - fino a 50 Aziende","POB.006"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_NuovaAttivazioneSubcliente(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttSubCli");
			a.Title = "Quale modulo desideri attivare?";
			a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("POB.001", "POB.001 - Portale Open Banking per AZIENDA con Espando Modulare o Italworking", "POB.001"),
			   new InputItem("POB.021", "POB.021 - Portale Open Banking per AZIENDA", "POB.021")
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_UpgradeSenzaB2B(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradeOpenBanking");
			a.Title = "Che tipo di attivazione vuoi effettuare?";
			a.TestoRiepilogo = "Tipo di attivazione da effettuare";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("commercialista", "Commercialista","PDS.COMM"),
			   new InputItem("azienda", "Azienda","PDS.AZI"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("upgradeComm");
			b1.Condition.IfOutputContainsItem("commercialista");

			Branch b2 = a.CreateBranchTo("upgradeAzi");
			b2.Condition.IfOutputContainsItem("azienda");
		}

		private void _AddActivity_UpgradeComm(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradeComm");
			a.Title = "Quale modulo desideri attivare?";
			a.TestoRiepilogo = "Modulo da attivare";

			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'upgradePOBComm','Text':'POB.007 - Portale Open Banking per Commercialista - 1 Azienda aggiuntiva','DataType':'integer','MinValue':1,'MaxValue':4,'DefaultValue':1, 'Tag':'POB.007'}"),
				new InputItem("{'Key':'upgradePOBComm','Text':'POB.008 - Portale Open Banking per Commercialista - 5 Aziende aggiuntive','DataType':'integer','MinValue':1,'MaxValue':99,'DefaultValue':1, 'Tag':'POB.008'}"),
				new InputItem("{'Key':'upgradePOBComm','Text':'POB.009 - Portale Open Banking - 1 Titolare/Delegato Conto Aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':4,'DefaultValue':1, 'Tag':'POB.009'}"),
				new InputItem("{'Key':'upgradePOBComm','Text':'POB.010 - Portale Open Banking - 5 Titolare/Delegato Conto Aggiuntivo','DataType':'integer','MinValue':1, 'MaxValue':99, 'DefaultValue':1, 'Tag':'POB.010'}"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}


		private void _AddActivity_UpgradeAzi(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradeAzi");
			a.Title = "Quale modulo desideri attivare?";
			a.TestoRiepilogo = "Modulo da attivare";

			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'upgradePOBAzi','Text':'POB.003 - Portale Open Banking per AZIENDA CAPOGRUPPO con Espando Modulare o Italworking – 1 Azienda aggiuntiva','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1, 'Tag':'POB.003'}"),
				new InputItem("{'Key':'upgradePOBAzi','Text':'POB.023 - Portale Open Banking per AZIENDA CAPOGRUPPO - 1 Azienda aggiuntiva','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1, 'Tag':'POB.023'}"),
				new InputItem("{'Key':'upgradePOBAzi','Text':'POB.009 - Portale Open Banking - 1 Titolare/Delegato Conto Aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':4,'DefaultValue':1, 'Tag':'POB.009'}"),
				new InputItem("{'Key':'upgradePOBAzi','Text':'POB.010 - Portale Open Banking - 5 Titolare/Delegato Conto Aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':99,'DefaultValue':1, 'Tag':'POB.010'}"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_UpgradeSubcliente(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradeSubCli");
			a.Title = "Quale modulo desideri attivare?";
			a.TestoRiepilogo = "Modulo da attivare";

			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'upgradePOBSubCli','Text':'POB.009 - Portale Open Banking - 1 Titolare/Delegato Conto Aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':4,'DefaultValue':1, 'Tag':'POB.009'}"),
				new InputItem("{'Key':'upgradePOBSubCli','Text':'POB.010 - Portale Open Banking - 5 Titolare/Delegato Conto Aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':4,'DefaultValue':1, 'Tag':'POB.010'}"),
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
				 new InputItem("{'Key':'uploadFile','Text':'Caricare un file PDF','DataType':'blob', 'Tag':'Blob'}"),
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
