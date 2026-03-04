using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowRentri : Workflow
	{
		//private bool possiedeContratto { get; set; }

		private Action<StateContext> _DrawPage { get; set; }
		private int tipoLicenza { get; set; } //0 -> niente, 1-> comm, 2 -> azi

		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowRentri(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> activities = GetActivities(typeof(WorkflowRentri));

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
			else if (tipoLicenza == 1)
			{
				b1 = a.CreateBranchTo("attCOMM");
				b1.Condition.IfOutputContainsItem("nuova");
				Branch b2 = a.CreateBranchTo("soggUpgrade");
				b2.Condition.IfOutputContainsItem("upgrade");
			}
			else if (tipoLicenza == 2)
			{
				b1 = a.CreateBranchTo("attAZI");
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
				new InputItem("prof", "Commercialista / Associazione", "PDS.COMM"),
				new InputItem("azi", " Azienda Singola / Azienda Capogruppo", "PDS.AZI"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("attCOMM");
			b1.Condition.IfOutputContainsItem("prof");

			Branch b2 = a.CreateBranchTo("attAZI");
			b2.Condition.IfOutputContainsItem("azi");
		}

		private void _AddActivity_NuovoCOMM(Workflow wf)
		{
			Activity a = wf.CreateActivity("attCOMM");
			a.Title = "Quale modulo vuoi attivare?";
			a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("BYWTOOLIPRENTRI", "BYWTOOLIPRENTRI - [BYTE] Byte - Win-Toolip - RENTRI: Bundle", "BYWTOOLIPRENTRI;DOCS.COMCSBASESH;DOCS.SCS100;FIRM.AUTO400", true),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_NuovoAzi(Workflow wf)
		{
			Activity a = wf.CreateActivity("attAZI");
			a.Title = "Quale modulo vuoi attivare?";
			a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("BYWTOOLIPRENTRI", "BYWTOOLIPRENTRI - [BYTE] Byte - Win-Toolip - RENTRI: Bundle", "BYWTOOLIPRENTRI;DOCS.AZICSBASE;DOCS.SCS100;FIRM.AUTO400", true),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_Upgrade(Workflow wf)
		{
			Activity a = wf.CreateActivity("soggUpgrade");
			a.Title = "Seleziona l'upgrade che desideri attivare?<span style='font-size:20px'>1 di 2</span>";
			a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				new InputItem("FIRM.AUTO400AGG", "FIRM.AUTO400AGG - Firma Qualificata Automatica - 400 firme aggiuntive", "FIRM.AUTO400AGG"),
				new InputItem("DOCS.SCS100", "DOCS.SCS100 - Condiviso Conservazione - fino a 100 MB", "DOCS.SCS100"),
				new InputItem("DOCS.SCS200", "DOCS.SCS200 - Condiviso Conservazione - fino a 200 MB", "DOCS.SCS200"),
				new InputItem("DOCS.SCSU500", "DOCS.SCSU500 - Condiviso Conservazione - ulteriori 500 MB", "DOCS.SCSU500"),
				new InputItem("DOCS.SCS1000", "DOCS.SCS1000 - Condiviso Conservazione - fino a 1.000 MB", "DOCS.SCS1000"),
				new InputItem("DOCS.NCCSU1", "DOCS.NCCSU1 - Condiviso Conservazione - 1 ulteriore Partita IVA", "DOCS.NCCSU1"),
				new InputItem("DOCS.NCCSU100", "DOCS.NCCSU100 - Condiviso Conservazione - 100 ulteriori Partite IVA", "DOCS.NCCSU100"),
			}));
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;
			Branch b1 = a.CreateBranchTo("soggUpgradeRentripag2");
		}

		private void _AddActivity_UpgradePag2(Workflow wf)
		{
			Activity a = wf.CreateActivity("soggUpgradeRentripag2");
			a.Title = "Seleziona l'upgrade che desideri attivare?<span style='font-size:20px'>2 di 2</span>";
			a.TestoRiepilogo = "Modulo da attivare:";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'upgradeRentri','Text':'FIRM.AUTO400 - Firma Qualificata Automatica - 400 firme e 1 Soggetto','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1,'Tag':'FIRM.AUTO400'}"),
			}));
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

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
