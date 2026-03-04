using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowRDC : Workflow
	{
		private Action<StateContext> _DrawPage { get; set; }

		private int tipoLicenza { get; set; } // 0 -> niente, 1 -> comm, 2 -> azi
		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowRDC(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
		{
			_DrawPage = drawPage;

			this.tipoLicenza = tipoLicenza;

			List<string> activities = GetActivities(typeof(WorkflowRDC));

			//this.possiedeContratto = possiedeContratto;

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoModuli(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoModuli");
			a.Title = "Quale tipo di moduli vuoi abilitare?";
			a.TestoRiepilogo = "Tipo di moduli:";
			//a.Description = "Breve descrizione...";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("Nuovo", "Nuova attivazione"),
				new InputItem("Upgrade", "Upgrade")
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = null;

			switch (tipoLicenza)
			{
				case 0:
					b1 = a.CreateBranchTo("tipoSogg");
					b1.Condition.IfOutputContainsItem("Nuovo");
					break;
				case 1:
					b1 = a.CreateBranchTo("tipoModuliProf");
					b1.Condition.IfOutputContainsItem("Nuovo");
					break;
				case 2:
					b1 = a.CreateBranchTo("tipoModuliAzi");
					b1.Condition.IfOutputContainsItem("Nuovo");
					break;
			}

			Branch b2 = a.CreateBranchTo("tipoModuliUpgrade");
			b2.Condition.IfOutputContainsItem("Upgrade");
		}

		private void _AddActivity_Soggetto(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoSogg");
			a.Title = "Quale tipo di soggetto vuoi abilitare?";
			a.TestoRiepilogo = "Tipo di soggetto:";
			//a.Description = "Breve descrizione...";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("prof", "Professionista"),
				new InputItem("azi", "Azienda"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("tipoModuliProf");
			b1.Condition.IfOutputContainsItem("prof");

			Branch b2 = a.CreateBranchTo("tipoModuliAzi");
			b2.Condition.IfOutputContainsItem("azi");
		}

		private void _AddActivity_TipoModuliProf(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoModuliProf");
			a.Title = "Quale tipo di moduli vuoi abilitare?";
			a.TestoRiepilogo = "Tipo di moduli:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("6700001", "6700001 - Servizio Responsabile della Conservazione per Studi", "6700001", true),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_TipoModuliAzi(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoModuliAzi");
			a.Title = "Quale tipo di moduli vuoi abilitare?";
			a.TestoRiepilogo = "Tipo di moduli:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("6700002", "6700002 - Servizio Responsabile della Conservazione per Aziende con gestionale Bluenext", "6700002"),
				new InputItem("6700003", "6700003 - Servizio Responsabile della Conservazione per Aziende Capogruppo con gestionale Bluenext", "6700003"),
				new InputItem("6700004", "6700004 - Servizio Responsabile della Conservazione per Aziende", "6700004"),
				new InputItem("6700005", "6700005 - Servizio Responsabile della Conservazione per Aziende Capogruppo", "6700005"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_TipoModuliUpgrade(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoModuliUpgrade");
			a.Title = "Quale tipo di moduli vuoi abilitare?";
			a.TestoRiepilogo = "Tipo di moduli:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("6700099", "6700099 - Verifica Manuale Conservazione 2022-2023 (Una tantum)", "6700099", true),
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