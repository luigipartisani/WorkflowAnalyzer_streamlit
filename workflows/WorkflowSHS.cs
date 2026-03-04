using BN.Licenze.Serv_AccessoDati.Model.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
	public class WorkflowSHS : Workflow
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

		public WorkflowSHS(string key, string title, Action<StateContext> drawPage) : base(key, title)
		{
			_DrawPage = drawPage;

			List<string> methods = ShowMethods(typeof(WorkflowSHS));

			foreach (string s in methods)
			{
				MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoCliente(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoCliente");
			a.Title = "Che tipo di cliente desideri attivare?";
			a.TestoRiepilogo = "Tipo di cliente da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("aziTit", "Azienda titolare"),
				new InputItem("commTitolare", "Commercialista titolare"),
				new InputItem("subcliente", "Subcliente"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("sceltaTipoAttivazione");
			b1.Condition.IfOutputContainsItem("aziTit");
			Branch b2 = a.CreateBranchTo("sceltaTipoAttivazione");
			b2.Condition.IfOutputContainsItem("commTitolare");
			Branch b3 = a.CreateBranchTo("subClientePITitolare");
			b3.Condition.IfOutputContainsItem("subcliente");
		}

		private void _AddActivity_AziCollegataPITitolare(Workflow wf)
		{
			Activity a = wf.CreateActivity("subClientePITitolare");
			a.Title = "Partita iva del titolare della licenza";
			a.TestoRiepilogo = "Partita iva del titolare della licenza";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'PITitAziCollAd','Text':'PI titolare','DataType':'string'}"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("sceltaTipoAttivazione");
		}

		private void _AddActivity_TipoAttivazione(Workflow wf)
		{
			Activity a = wf.CreateActivity("sceltaTipoAttivazione");
			a.Title = "Che tipo di attivazione desideri attivare?";
			a.TestoRiepilogo = "Tipo di cliente da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("nuova", "Nuova postazione"),
				new InputItem("migrazione", "Migrazione"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchToSummary();
			b1.Condition.IfOutputContainsItem("nuova");

			Branch b2 = a.CreateBranchTo("datiSHS");
			b2.Condition.IfOutputContainsItem("migrazione");
		}

		private void _AddActivity_DatiSHS(Workflow wf)
		{
			Activity a = wf.CreateActivity("datiSHS");
			a.Title = "Dati aggiuntivi";
			a.TestoRiepilogo = "Dati aggiuntivi:";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				 new InputItem("{'Key':'datiSHS','Text':'Codice postazione','DataType':'text', 'Tag':'postazione'}"),
				 new InputItem("{'Key':'datiSHS','Text':'Codice istituto','DataType':'text', 'Tag':'istituto'}"),
				 new InputItem("{'Key':'datiSHS','Text':'Codice CUC','DataType':'text', 'Tag':'cuc'}"),
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
