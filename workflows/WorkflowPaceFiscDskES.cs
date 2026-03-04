using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowPaceFiscDskES : Workflow
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

		public WorkflowPaceFiscDskES(string key, string title, Action<StateContext> drawPage) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> methods = ShowMethods(typeof(WorkflowPaceFiscDskES));

			foreach (string s in methods)
			{
				MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoGestioneNuovo(Workflow wf)
		{
			Activity a = wf.CreateActivity("modulo");
			a.Title = "Modulo da attivare";
			a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("6568103.ES", "6568103 - Pace Fiscale 2023","6568103.ES",true)
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
			a.Title = "La procedura di attivazione si è conclusa";
			a.DrawPage = _DrawPage;
		}
	}
}