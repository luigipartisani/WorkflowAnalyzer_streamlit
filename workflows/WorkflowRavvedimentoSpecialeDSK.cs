using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowRavvedimentoSpecialeDSK : Workflow
	{
		private Action<StateContext> _DrawPage { get;set; }

		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowRavvedimentoSpecialeDSK(string key, string title, Action<StateContext> drawPage) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> activities = GetActivities(typeof(WorkflowRavvedimentoSpecialeDSK));

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_Modulo(Workflow wf)
		{
			Activity a = wf.CreateActivity("modulo");
			a.Title = "Modulo da attivare";
			a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				 new InputItem("RS.DESK","RS.DESK - Ravvedimento Speciale: Gestione", "RS.DESK", true),
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
				 new InputItem("{'Key':'uploadFile','Text':'Caricare un file PDF','DataType':'blob', 'Tag':'Blob'}")
			}));
			a.DrawPage = _DrawPage;

			//Branch b1 = a.CreateBranchTo("note");
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