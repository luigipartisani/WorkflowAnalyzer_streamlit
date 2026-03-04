using Ext.Net.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowAIDE : Workflow
	{
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

		public WorkflowAIDE(string key, string title, Action<StateContext> drawPage) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> activities = GetActivities(typeof(WorkflowAIDE));

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoLicenza(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoAttivazione");
			a.Title = "Tipo di attivazione";
			a.TestoRiepilogo = "Tipo di attivazione:";
			//a.Description = "Breve descrizione...";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("nuova","Nuova attivazione"),
				new InputItem("upgrade","Upgrade")
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("nuovaAttivazione");
			b1.Condition.IfOutputContainsItem("nuova");

			Branch b2 = a.CreateBranchTo("upgradeAttivazione");
			b2.Condition.IfOutputContainsItem("upgrade");
		}

		private void _AddActivity_NuovaAttivazione(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttivazione");
			a.Title = "Quale tipologia di servizio desideri attivare?<span style='font-size:20px'>1 di 2</span>";
			a.TestoRiepilogo = "Tipo di attivazione";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("AD.DE.BA.CA", "AD.DE.BA.CA - AI Data Extraction Basic Canone Pagine Servizio (include eventuali Add-On attivati)", "AD.DE.BA.CA;AD.DE.BN"),
			   new InputItem("AD.DE.PR.CA", "AD.DE.PR.CA - AI Data Extraction Premium Canone Pagine Servizio (include eventuali Add-On attivati)", "AD.DE.PR.CA;AD.DE.BN"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("nuovaAttivazionePag2AIDE");
		}

		private void _AddActivity_NuovaAttivazionePag2(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttivazionePag2AIDE");
			a.Title = "Quante pagine devono essere disponibili?<span style='font-size:20px'>2 di 2</span>";
			a.TestoRiepilogo = "Tipo di attivazione";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
			   new InputItem("{'Key':'pagNuovoAIDE','Text':'Numero pagine','DataType':'integer','MinValue':1,'MaxValue':999999,'DefaultValue':1,'Tag':'xpagNuovoAIDE'}")
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_UpgradeAttivazione(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradeAttivazione");
			a.Title = "Quale tipologia di servizio desideri attivare?<span style='font-size:20px'>1 di 2</span>";
			a.TestoRiepilogo = "Tipo di attivazione";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("AD.DE.BA.CA", "AD.DE.BA.CA - AI Data Extraction Basic Canone Pagine Servizio (include eventuali Add-On attivati)", "AD.DE.BA.CA"),
			   new InputItem("AD.DE.PR.CA", "AD.DE.PR.CA - AI Data Extraction Premium Canone Pagine Servizio (include eventuali Add-On attivati)", "AD.DE.PR.CA"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("upgradeAttivazionePag2AIDE");
		}

		private void _AddActivity_UpgradeAttivazionePag2(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradeAttivazionePag2AIDE");
			a.Title = "Quante pagine devono essere disponibili?<span style='font-size:20px'>2 di 2</span>";
			a.TestoRiepilogo = "Tipo di attivazione";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
			   new InputItem("{'Key':'pagNuovoAIDE','Text':'Numero pagine','DataType':'integer','MinValue':1,'MaxValue':999999,'DefaultValue':1,'Tag':'xpagNuovoAIDE'}")
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
