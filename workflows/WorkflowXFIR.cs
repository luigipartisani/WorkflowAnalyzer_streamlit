using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowXFIR : Workflow
	{
		private Action<StateContext> _DrawPage { get; set; }

		private int tipoLicenza { get; set; } // 0 -> niente, 1 -> comm, 2 -> azi
		private int tipoLink { get; set; } // 0 -> comm, 3 -> azi
		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowXFIR(string key, string title, Action<StateContext> drawPage, int tipoLicenza, int tipoLink) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> activities = GetActivities(typeof(WorkflowXFIR));

			this.tipoLicenza = tipoLicenza;

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoAttivazione(Workflow wf)
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

			Branch b1 = null;
			if (tipoLicenza == 0)
			{
				b1 = a.CreateBranchTo("sogg");
				b1.Condition.IfOutputContainsItem("nuova");
				Branch b2 = a.CreateBranchTo("soggUpgrade");
				b2.Condition.IfOutputContainsItem("upgrade");
			}
			else if (tipoLicenza == 1 || tipoLink == 0)
			{
				b1 = a.CreateBranchTo("xfirCOMM");
				b1.Condition.IfOutputContainsItem("nuova");
				Branch b2 = a.CreateBranchTo("soggUpgrade");
				b2.Condition.IfOutputContainsItem("upgrade");
			}
			else if (tipoLicenza == 2 || tipoLink == 3)
			{
				b1 = a.CreateBranchTo("xfirAZI");
				b1.Condition.IfOutputContainsItem("nuova");
				Branch b2 = a.CreateBranchTo("soggUpgrade");
				b2.Condition.IfOutputContainsItem("upgrade");
			}
		}

		private void _AddActivity_TipoSoggetto(Workflow wf)
		{
			Activity a = wf.CreateActivity("sogg");
			a.Title = "Quale tipo di soggetto vuoi abilitare?";
			a.TestoRiepilogo = "Tipo di soggetto:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("prof", "Professionista","PDS.COMM"),
				new InputItem("azi", "Azienda","PDS.AZI"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("xfirCOMM");
			b1.Condition.IfOutputContainsItem("prof");

			Branch b2 = a.CreateBranchTo("xfirAZI");
			b2.Condition.IfOutputContainsItem("azi");
		}

		private void _AddActivity_XFIRComm(Workflow wf)
		{
			Activity a = wf.CreateActivity("xfirCOMM");
			a.Title = "Quale modulo vuoi attivare?";
			a.TestoRiepilogo = "Modulo da attivare:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("BYTOOLIPXFIR.2500", "BYTOOLIPXFIR.2500 - [BYTE] Win-Toolip - fino a 2.500 xFIR tramitati / anno", "BYTOOLIPXFIR.2500;DOCS.COMCSBASESH;DOCS.SCS70"),
				new InputItem("BYTOOLIPXFIR.4000", "BYTOOLIPXFIR.4000 - [BYTE] Win-Toolip - fino a 4.000 xFIR tramitati / anno", "BYTOOLIPXFIR.4000;DOCS.COMCSBASESH;DOCS.SCS100"),
				new InputItem("BYTOOLIPXFIR.10000", "BYTOOLIPXFIR.10000 - [BYTE] Win-Toolip - fino a 10.000 xFIR tramitati / anno", "BYTOOLIPXFIR.10000;DOCS.COMCSBASESH;DOCS.SCS250"),
				new InputItem("BYTOOLIPXFIR.20000", "BYTOOLIPXFIR.20000 - [BYTE] Win-Toolip - fino a 20.000 xFIR tramitati / anno", "BYTOOLIPXFIR.20000;DOCS.COMCSBASESH;DOCS.SCS500"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_XFIRAzi(Workflow wf)
		{
			Activity a = wf.CreateActivity("xfirAZI");
			a.Title = "Quale modulo vuoi attivare?";
			a.TestoRiepilogo = "Modulo da attivare:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("BYTOOLIPXFIR.2500", "BYTOOLIPXFIR.2500 - [BYTE] Win-Toolip - fino a 2.500 xFIR tramitati / anno ", "BYTOOLIPXFIR.2500;DOCS.AZICSBASE;DOCS.SCS70"),
				new InputItem("BYTOOLIPXFIR.4000", "BYTOOLIPXFIR.4000 - [BYTE] Win-Toolip - fino a 4.000 xFIR tramitati / anno", "BYTOOLIPXFIR.4000;DOCS.AZICSBASE;DOCS.SCS100"),
				new InputItem("BYTOOLIPXFIR.10000", "BYTOOLIPXFIR.10000 - [BYTE] Win-Toolip - fino a 10.000 xFIR tramitati / anno", "BYTOOLIPXFIR.10000;DOCS.AZICSBASE;DOCS.SCS250"),
				new InputItem("BYTOOLIPXFIR.20000", "BYTOOLIPXFIR.20000 - [BYTE] Win-Toolip - fino a 20.000 xFIR tramitati / anno", "BYTOOLIPXFIR.20000;DOCS.AZICSBASE;DOCS.SCS500"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_Upgrade(Workflow wf)
		{
			Activity a = wf.CreateActivity("soggUpgrade");
			a.Title = "Seleziona l'upgrade che desideri attivare";
			a.TestoRiepilogo = "Upgrade da attivare:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("BYTOOLIPXFIR.4000", "BYTOOLIPXFIR.4000 - [BYTE] Win-Toolip - fino a 4.000 xFIR tramitati / anno", "BYTOOLIPXFIR.4000;DOCS.SCS100"),
				new InputItem("BYTOOLIPXFIR.10000", "BYTOOLIPXFIR.10000 - [BYTE] Win-Toolip - fino a 10.000 xFIR tramitati / anno", "BYTOOLIPXFIR.10000;DOCS.SCS250"),
				new InputItem("BYTOOLIPXFIR.20000", "BYTOOLIPXFIR.20000 - [BYTE] Win-Toolip - fino a 20.000 xFIR tramitati / anno", "BYTOOLIPXFIR.20000;DOCS.SCS500"),
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