using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowDocsCS : Workflow
	{
		private Action<StateContext> _DrawPage { get; set; }
		private int tipoLicenza { get; set; } //0 -> niente, 1-> comm, 2 -> azi
		private int tipoLink { get; set; }

		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowDocsCS(string key, string title, Action<StateContext> drawPage, int tipoLicenza, int tipoLink) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> activities = GetActivities(typeof(WorkflowDocsCS));

			this.tipoLicenza = tipoLicenza;
			this.tipoLink = tipoLink;

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
			if (tipoLink == -1)
			{
				b1 = a.CreateBranchTo("sogg");
				b1.Condition.IfOutputContainsItem("nuova");
				Branch b2 = a.CreateBranchTo("soggUpgrade");
				b2.Condition.IfOutputContainsItem("upgrade");
			}
			else if (tipoLink == 0)
			{
				b1 = a.CreateBranchTo("docsCOMM");
				b1.Condition.IfOutputContainsItem("nuova");
				Branch b2 = a.CreateBranchTo("soggUpgrade");
				b2.Condition.IfOutputContainsItem("upgrade");
			}
			else if (tipoLink == 1 || tipoLink == 2 || tipoLink == 3)
			{
				b1 = a.CreateBranchTo("docsAZI");
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

			Branch b1 = a.CreateBranchTo("docsCOMM");
			b1.Condition.IfOutputContainsItem("prof");

			Branch b2 = a.CreateBranchTo("docsAZI");
			b2.Condition.IfOutputContainsItem("azi");
		}

		private void _AddActivity_DocsComm(Workflow wf)
		{
			Activity a = wf.CreateActivity("docsCOMM");
			a.Title = "Seleziona il modulo base:";
			a.TestoRiepilogo = "Modulo base:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("DOCS.COMCSBASESH", "DOCS.COMCSBASESH - Condiviso Conservazione - Modulo base per Studi (per Software House)", "DOCS.COMCSBASESH"),
				new InputItem("DOCS.COMCSBASESH_S", "DOCS.COMCSBASESH_S -  Condiviso Conservazione - Modulo base per Studi con conteggio a SPAZIO (per Sofware House)", "DOCS.COMCSBASESH_S"),
				new InputItem("DOCS.COMCSBASE", "DOCS.COMCSBASE Condiviso Conservazione - Modulo base per Studi", "DOCS.COMCSBASE")
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("numeroPagineDaConservare");
		}

		private void _AddActivity_DocsAzi(Workflow wf)
		{
			Activity a = wf.CreateActivity("docsAZI");

			a.Title = "Seleziona il modulo base:";
			a.TestoRiepilogo = "Modulo base:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("DOCS.AZICSBASESH", "DOCS.AZICSBASESH - Condiviso Conservazione - Modulo base per Aziende (per Software House)", "DOCS.AZICSBASESH"),
				new InputItem("DOCS.AZICSBASESH_S", "DOCS.AZICSBASESH_S - Condiviso Conservazione - Modulo base per Aziende con conteggio a SPAZIO (per Software House)", "DOCS.AZICSBASESH_S"),
				new InputItem("DOCS.AZICSBASE", "DOCS.AZICSBASE Condiviso Conservazione - Modulo base per Aziende", "DOCS.AZICSBASE")
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("numeroPagineDaConservare");
		}

		private void _AddActivity_NumeroPagineDaConservare(Workflow wf)
		{
			Activity a = wf.CreateActivity("numeroPagineDaConservare");

			a.Title = "Seleziona il numero di pagine che desideri conservare:";
			a.TestoRiepilogo = "Numero di pagine:";

			a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				new InputItem("DOCS.NPCS5"  ,"DOCS.NPCS5 - Condiviso Conservazione - fino a 5.000 pagine", "DOCS.NPCS5"  ),
				new InputItem("DOCS.NPCS8"  ,"DOCS.NPCS8 - Condiviso Conservazione - fino a 8.000 pagine", "DOCS.NPCS8"  ),
				new InputItem("DOCS.NPCS10" ,"DOCS.NPCS10 - Condiviso Conservazione - fino a 10.000 pagine","DOCS.NPCS10" ),
				new InputItem("DOCS.NPCS15" ,"DOCS.NPCS15 - Condiviso Conservazione - fino a 15.000 pagine","DOCS.NPCS15" ),
				new InputItem("DOCS.NPCS20" ,"DOCS.NPCS20 - Condiviso Conservazione - fino a 20.000 pagine","DOCS.NPCS20" ),
				new InputItem("DOCS.NPCS30" ,"DOCS.NPCS30 - Condiviso Conservazione - fino a 30.000 pagine","DOCS.NPCS30" ),
				new InputItem("DOCS.NPCS40" ,"DOCS.NPCS40 - Condiviso Conservazione - fino a 40.000 pagine","DOCS.NPCS40" ),
				new InputItem("DOCS.NPCS50" ,"DOCS.NPCS50 - Condiviso Conservazione - fino a 50.000 pagine","DOCS.NPCS50" ),
				new InputItem("DOCS.NPCS60" ,"DOCS.NPCS60 - Condiviso Conservazione - fino a 60.000 pagine","DOCS.NPCS60" ),
				new InputItem("DOCS.NPCS70" ,"DOCS.NPCS70 - Condiviso Conservazione - fino a 70.000 pagine","DOCS.NPCS70" ),
				new InputItem("DOCS.NPCS80" ,"DOCS.NPCS80 - Condiviso Conservazione - fino a 80.000 pagine","DOCS.NPCS80" ),
				new InputItem("DOCS.NPCS90" ,"DOCS.NPCS90 - Condiviso Conservazione - fino a 90.000 pagine","DOCS.NPCS90" ),
				new InputItem("DOCS.NPCS100","DOCS.NPCS100 - Condiviso Conservazione - fino a 100.000 pagine", "DOCS.NPCS100")
			}));
			a.DrawPage = _DrawPage;

			a.AllowNoChoice = true;
			Branch b1 = a.CreateBranchTo("numeroMBDaConservare");
		}

		private void _AddActivity_NumeroMBDaConservare(Workflow wf)
		{
			Activity a = wf.CreateActivity("numeroMBDaConservare");

			a.Title = "Seleziona i Mb che desideri conservare:";
			a.TestoRiepilogo = "Numero di MB:";

			a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				new InputItem("DOCS.SCS100"  ,"DOCS.SCS100 - Condiviso Conservazione - fino a 100 MB", "DOCS.SCS100"),
				new InputItem("DOCS.SCS200"  ,"DOCS.SCS200 - Condiviso Conservazione - fino a 200 MB", "DOCS.SCS200"),
				new InputItem("DOCS.SCS500"  ,"DOCS.SCS500 - Condiviso Conservazione - fino a 500 MB", "DOCS.SCS500"),
				new InputItem("DOCS.SCS1000"  ,"DOCS.SCS1000 - Condiviso Conservazione - fino a 1.000 MB", "DOCS.SCS1000"),
				new InputItem("DOCS.SCS5000"  ,"DOCS.SCS5000 - Condiviso Conservazione - fino a 5.000 MB", "DOCS.SCS5000"),
				new InputItem("DOCS.SCS10000"  ,"DOCS.SCS10000 - Condiviso Conservazione - fino a 10.000 MB", "DOCS.SCS10000"),
			}));

			a.DrawPage = _DrawPage;

			a.AllowNoChoice = true;
			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_soggUpgrade(Workflow wf)
		{
			Activity a = wf.CreateActivity("soggUpgrade");

			a.Title = "Seleziona l'upgrade che desideri attivare";
			a.TestoRiepilogo = "Upgrade da attivare:";

			a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				//new InputItem("DOCS.SCSU500"  ,"DOCS.SCSU500 - Condiviso Conservazione - ulteriori 500 MB", "DOCS.SCSU500"),
				new InputItem("DOCS.SCSU5000"  ,"DOCS.SCSU5000 - Condiviso Conservazione – ulteriori 5000 MB ", "DOCS.SCSU5000"),
				new InputItem("DOCS.NCCSU1"  ,"DOCS.NCCSU1 - Condiviso Conservazione - 1 ulteriore Partita IVA", "DOCS.NCCSU1"),
				new InputItem("DOCS.NCCSU100"  ,"DOCS.NCCSU100 - Condiviso Conservazione - 100 ulteriori Partite IVA", "DOCS.NCCSU100")
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
