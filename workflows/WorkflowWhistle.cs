using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowWhistle : Workflow
	{
		private Action<StateContext> _DrawPage { get; set; }

		private List<string> ShowMethods(Type type)
		{
			List<string> methods = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
			}

			return methods;
		}

		public WorkflowWhistle(string key, string title, Action<StateContext> drawPage) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> methods = ShowMethods(typeof(WorkflowWhistle));

			foreach (string s in methods)
			{
				MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoConfigurazione(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoConfigurazione");
			a.Title = "Quale configurazione desideri attivare?";
			a.TestoRiepilogo = "Configurazione da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("WSBL0050","WSBL0050 - Whistleblowing fino a 50 dipendenti","WSBL0050"),
				new InputItem("WSBL0100","WSBL0100 - Whistleblowing da 51 a 100 dipendenti","WSBL0100"),
				new InputItem("WSBL0250","WSBL0250 - Whistleblowing da 101 a 250 dipendenti","WSBL0250"),
				new InputItem("WSBL0500","WSBL0500 - Whistleblowing da 251 a 500 dipendenti","WSBL0500"),
				new InputItem("WSBL1000","WSBL1000 - Whistleblowing da 501 a 1000 dipendenti","WSBL1000"),
				new InputItem("WSBL9999","WSBL9999 - Whistleblowing oltre 1000 dipendenti","WSBL9999"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("sceltaSoggettiWhistle");
		}

		private void _AddActivity_SceltaSoggetti(Workflow wf)
		{
			Activity a = wf.CreateActivity("sceltaSoggettiWhistle");
			a.Title = "Sono stati indicati i soggetti abilitati ad accedere alla gestione (ODV)? <span style='font-size: 12px'>(Se si compilare i campi di seguito, altrimenti cliccare su Avanti)</span>";
			a.TestoRiepilogo = "Soggetti abilitati ad accedere alla gestione (ODV):";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				 new InputItem("{'Key':'datiSoggettiWhistle','Text':'Nome Cognome 1','DataType':'text', 'Tag':'nomeSoggettiWhistle1'}"),
				 //new InputItem("{'Key':'datiSoggettiWhistle','Text':'Cognome 1','DataType':'text', 'Tag':'cognomeSoggettiWhistle1'}"),
				 new InputItem("{'Key':'datiSoggettiWhistle','Text':'Indirizzo E-mail 1','DataType':'text', 'Tag':'mailSoggettiWhistle1'}"),
				 new InputItem("{'Key':'datiSoggettiWhistle','Text':'Nome Cognome 2','DataType':'text', 'Tag':'nomeSoggettiWhistle2'}"),
				 //new InputItem("{'Key':'datiSoggettiWhistle','Text':'Cognome 2','DataType':'text', 'Tag':'cognomeSoggettiWhistle2'}"),
				 new InputItem("{'Key':'datiSoggettiWhistle','Text':'Indirizzo E-mail 2','DataType':'text', 'Tag':'mailSoggettiWhistle2'}"),
				 new InputItem("{'Key':'datiSoggettiWhistle','Text':'Nome Cognome 3','DataType':'text', 'Tag':'nomeSoggettiWhistle3'}"),
				 //new InputItem("{'Key':'datiSoggettiWhistle','Text':'Cognome 3','DataType':'text', 'Tag':'cognomeSoggettiWhistle3'}"),
				 new InputItem("{'Key':'datiSoggettiWhistle','Text':'Indirizzo E-mail 3','DataType':'text', 'Tag':'mailSoggettiWhistle3'}"),
			}));
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

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
			a.Title = "La procedura di attivazione si è conclusa";
			a.DrawPage = _DrawPage;
		}
	}
}