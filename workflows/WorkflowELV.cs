using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowELV : Workflow
	{
		//private bool possiedeContratto { get; set; }

		private Action<StateContext> _DrawPage { get; set; }

		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowELV(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto*/) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> activities = GetActivities(typeof(WorkflowELV));

			//this.possiedeContratto = possiedeContratto;

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoAttivazione(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoAttivazione");
			a.Title = "Che tipo di attivazione vuoi effettuare?";
			//a.Description = "Breve descrizione...";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("nuovaAttivazione", "Nuova attivazione"),
			   new InputItem("upgrade", "Upgrade"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("nuovaAttivazionePag1");
			b1.Condition.IfOutputContainsItem("nuovaAttivazione");

			Branch b2 = a.CreateBranchTo("upgradeELV");
			b2.Condition.IfOutputContainsItem("upgrade");
		}

		private void _AddActivity_NuovaAttivazionePag1(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttivazionePag1");
			a.Title = "Quale configurazione desideri attivare?";
			a.TestoRiepilogo = "Tipo di configurazione";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("1ELV.01","1ELV.01 - Espando LegalVision - 1 azienda","1ELV.01"),
				new InputItem("1ELV.02","1ELV.02 - Espando LegalVision - Da 2 a 4 aziende ","1ELV.02"),
				new InputItem("1ELV.03","1ELV.03 - Espando LegalVision - Da 5 a10 aziende ","1ELV.03"),
				new InputItem("1ELV.04","1ELV.04 - Espando LegalVision - Oltre 10 aziende ","1ELV.04")
			}));
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("nuovaAttivazionePag2");
		}

		private void _AddActivity_NuovaAttivazionePag2(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttivazionePag2");
			a.Title = "La procedura fornisce già un'utenza per attivazione:<br/>desideri attivare utenze aggiuntive?";
			a.TestoRiepilogo = "Utenti aggiuntivi";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'utenteAggiuntivoNuovoELV','Text':'1ELV.05 - Espando LegalVision - utente aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'1ELV.05'}"),
			}));
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_UpgradeELV(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradeELV");
			a.Title = "La procedura fornisce già un'utenza per attivazione:<br/>desideri attivare utenze aggiuntive?";
			a.TestoRiepilogo = "Utenti aggiuntivi";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'utenteAggiuntivoUpgradeELV','Text':'1ELV.05 - Espando LegalVision - utente aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'1ELV.05'}"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_UploadPDF(Workflow wf)
		{
			Activity a = wf.CreateActivity("uploadFile");
			a.Title = "Carica il PDF del contratto";
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
