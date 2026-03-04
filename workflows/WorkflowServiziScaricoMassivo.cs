using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowServiziScaricoMassivo : Workflow
	{
		//private bool possiedeContratto { get; set; }

		private Action<StateContext> _DrawPage { get; set; }
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

		public WorkflowServiziScaricoMassivo(string key, string title, Action<StateContext> drawPage, int tipoLink) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> activities = GetActivities(typeof(WorkflowServiziScaricoMassivo));

			this.tipoLink = tipoLink;

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoLicenza(Workflow wf)
		{
			Activity a = null;
			
			if (tipoLink == 0)
			{
				a = wf.CreateActivity("lic");
				a.Title = "Che tipo di attivazione desideri effettuare?";
				a.TestoRiepilogo = "Tipo di attivazione:";

				a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("nuova","Nuova attivazione"),
				new InputItem("upgrade", "Upgrade"),
			}));
			}
			else
			{
				//a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				// new InputItem("nuova","Nuova attivazione"),
				//}));
				a = wf.CreateActivity("nuovaAttivazioneAzi");
				a.Title = "Quale configurazione desideri attivare?";
				a.TestoRiepilogo = "Configurazione da attivare";
				a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
					new InputItem("6008340","6008340  - Attivazione Certificato - Servizi di scarico firma qualificata","6008340;6008350.TECH",true,true),
					new InputItem("6008341","6008341 – 1 PdL - Servizi di scarico firma qualificata","6008341"),
					new InputItem("6008342","6008342 - Fino a 3 PdL - Servizi di scarico firma qualificata","6008342"),
					new InputItem("6008343","6008343 - Fino a 5 PdL - Servizi di scarico firma qualificata","6008343"),
					new InputItem("6008344","6008344 - Fino a 10 PdL - Servizi di scarico firma qualificata","6008344"),
					new InputItem("6008345","6008345 - Fino a 15 PdL - Servizi di scarico firma qualificata","6008345"),
					new InputItem("6008346","6008346 - Oltre 15 PdL - Servizi di scarico firma qualificata","6008346"),
					new InputItem("6008347","6008347 - Zero PdL - Servizi di scarico firma qualificata","6008347")
				}));
			}
			a.DrawPage = _DrawPage;

			Branch b1 = null;
			if (tipoLink == 0)
			{
				b1 = a.CreateBranchTo("nuovaAttivazioneComm");
				b1.Condition.IfOutputContainsItem("nuova");

				Branch b2 = a.CreateBranchTo("aziendaFiscOnline");
				b2.Condition.IfOutputContainsItem("upgrade");
			}
			else
			{
				//b1 = a.CreateBranchTo("nuovaAttivazioneAzi");
				//b1.Condition.IfOutputContainsItem("nuova");
				b1 = a.CreateBranchTo("uploadFile");
			}
		}

		private void _AddActivity_NuovaAttivazioneComm(Workflow wf)
		{
			Activity a = wf.CreateActivity("nuovaAttivazioneComm");
			a.Title = "Quale configurazione desideri attivare?";
			a.TestoRiepilogo = "Configurazione da attivare";
			a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
				new InputItem("6008340","6008340  - Attivazione Certificato - Servizi di scarico firma qualificata","6008340;6008349.TECH",true,true),
				new InputItem("6008341","6008341 – 1 PdL - Servizi di scarico firma qualificata","6008341"),
				new InputItem("6008342","6008342 - Fino a 3 PdL - Servizi di scarico firma qualificata","6008342"),
				new InputItem("6008343","6008343 - Fino a 5 PdL - Servizi di scarico firma qualificata","6008343"),
				new InputItem("6008344","6008344 - Fino a 10 PdL - Servizi di scarico firma qualificata","6008344"),
				new InputItem("6008345","6008345 - Fino a 15 PdL - Servizi di scarico firma qualificata","6008345"),
				new InputItem("6008346","6008346 - Oltre 15 PdL - Servizi di scarico firma qualificata","6008346"),
				new InputItem("6008347","6008347 - Zero PdL - Servizi di scarico firma qualificata","6008347")
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("aziendaFiscOnline");
		}

		private void _AddActivity_AziendaFiscOnline(Workflow wf)
		{
			Activity a = wf.CreateActivity("aziendaFiscOnline");
			a.Title = "Sono presenti aziende con credenziali FiscOnline?<br/><span style='font-size:12px'>Se non presenti, non compilare il campo e cliccare su AVANTI </span>";
			a.TestoRiepilogo = "Azienda con credenziali FiscOnline";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'aziendaCredFiscOnline','Text':'6008348 - Azienda FiscOnline aggiuntiva su Studio (.cad)','DataType':'integer', 'MinValue':1, 'MaxValue':20, 'Tag':'6008348'}"),
			}));

			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		//private void _AddActivity_NuovaAttivazioneAzi(Workflow wf)
		//{
		//	Activity a = wf.CreateActivity("nuovaAttivazioneAzi");
		//	a.Title = "Quale configurazione desideri attivare?";
		//	a.TestoRiepilogo = "Configurazione da attivare";
		//	a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
		//		new InputItem("6008340","6008340  - Attivazione Certificato - Servizi di scarico firma qualificata","6008340;6008350.TECH",true,true),
		//		new InputItem("6008341","6008341 – 1 PdL - Servizi di scarico firma qualificata","6008341"),
		//		new InputItem("6008342","6008342 - Fino a 3 PdL - Servizi di scarico firma qualificata","6008342"),
		//		new InputItem("6008343","6008343 - Fino a 5 PdL - Servizi di scarico firma qualificata","6008343"),
		//		new InputItem("6008344","6008344 - Fino a 10 PdL - Servizi di scarico firma qualificata","6008344"),
		//		new InputItem("6008345","6008345 - Fino a 15 PdL - Servizi di scarico firma qualificata","6008345"),
		//		new InputItem("6008346","6008346 - Oltre 15 PdL - Servizi di scarico firma qualificata","6008346"),
		//		new InputItem("6008347","6008347 - Zero PdL - Servizi di scarico firma qualificata","6008347")
		//	}));
		//	a.DrawPage = _DrawPage;

		//	Branch b1 = a.CreateBranchTo("uploadFile");
		//}

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
