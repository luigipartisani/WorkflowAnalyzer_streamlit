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
	public class WorkflowSMPL : Workflow
	{
		private Action<StateContext> _DrawPage { get; set; }
		private int tipoLicenza { get; set; } // 0 -> non c'è su cat inv reg, 1 ->  c'è su cat inv reg

		private List<string> GetActivities(Type type)
		{
			List<string> activities = new List<string>();

			foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
			}

			return activities;
		}

		public WorkflowSMPL(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
		{
			_DrawPage = drawPage;
			
			this.tipoLicenza = tipoLicenza;

			List<string> activities = GetActivities(typeof(WorkflowSMPL));

			foreach (string a in activities)
			{
				MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
				m.Invoke(this, new object[] { this });
			}
		}

		private void _AddActivity_TipoLicenza(Workflow wf)
		{
			Activity a = wf.CreateActivity("lic");
			a.Title = "Che tipo di licenza desideri attivare?";
			a.TestoRiepilogo = "Tipo di licenza da attivare:";

			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				 //new InputItem("Demo","Demo"),
				  new InputItem("Demo", "Demo - fino al " + DateTime.Now.AddDays(15).ToShortDateString()),
				  new InputItem("Standard", "Standard"),
			}));

			a.DrawPage = _DrawPage;

			Branch b1 = null;
			if (tipoLicenza == 1)
			{
				b1 = a.CreateBranchTo("tipoAttivazione");
			}
			else if (tipoLicenza == 0)
			{
				b1 = a.CreateBranchTo("tipoAttivazione");
				b1.Condition.IfOutputContainsItem("Demo");

				Branch b2 = a.CreateBranchTo("attivazioneNoB2B");
				b2.Condition.IfOutputContainsItem("Standard");
			}
		}

		private void _AddActivity_attivazioneNoB2B(Workflow wf)
		{
			Activity a = wf.CreateActivity("attivazioneNoB2B");
			a.Title = "Nessun Portale dei Servizi attivo, impossibile procedere";
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;
			a.CreateBranchToOutcome();
		}


		private void _AddActivity_TipoAttivazione(Workflow wf)
		{
			Activity a = wf.CreateActivity("tipoAttivazione");
			a.Title = "Che tipo di attivazione desideri effettuare?";
			a.TestoRiepilogo = "Tipo di attivazione";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
			   new InputItem("bundle", "Bundle"),
			   new InputItem("upgrade", "Upgrade"),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("bundle");
			b1.Condition.IfOutputContainsItem("bundle");

			Branch b2 = a.CreateBranchTo("upgrade");
			b2.Condition.IfOutputContainsItem("upgrade");
		}

		private void _AddActivity_TipoAttivazioneBundle(Workflow wf)
		{
			Activity a = wf.CreateActivity("bundle");
			a.Title = "Che tipo di bundle desideri attivare?";
			a.TestoRiepilogo = "Tipo di bundle";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				 new InputItem("SMPL.CMM","SMPL.CMM - Bundle Commerciale","SMPL.CMM"),
				 new InputItem("SMPL.CMP","SMPL.CMP - Bundle Commerciale Plus" ,"SMPL.CMP" ),
				 new InputItem("SMPL.SRV","SMPL.SRV - Bundle Service" ,"SMPL.SRV" ),
				 new InputItem("SMPL.SRP","SMPL.SRP - Bundle Service Plus" ,"SMPL.SRP" ),
				 new InputItem("SMPL.PRO","SMPL.PRO - Bundle Produzione","SMPL.PRO"),
				 new InputItem("SMPL.FLL","SMPL.FLL - Bundle Full" ,"SMPL.FLL" ),
			}));
			a.DrawPage = _DrawPage;

			Branch b1 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_TipoAttivazioneUpgrade(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgrade");
			a.Title = "Quale upgrade desideri attivare?<span style='font-size:16px'>1/2</span>";
			a.TestoRiepilogo = "Upgrade da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("SMPL.AGGCMM","SMPL.AGGCMM -  Posto di lavoro aggiuntivo (Bundle Commerciale)","SMPL.AGGCMM"),
				new InputItem("SMPL.AGGCMP","SMPL.AGGCMP -  Posto di lavoro aggiuntivo (Bundle Commerciale Plus)" ,"SMPL.AGGCMP"),
				new InputItem("SMPL.AGGSRV","SMPL.AGGSRV -  Posto di lavoro aggiuntivo (Bundle Service)" ,"SMPL.AGGSRV"),
				new InputItem("SMPL.AGGSRP","SMPL.AGGSRP - Posto di lavoro aggiuntivo (Bundle Service Plus)" ,"SMPL.AGGSRP"),
				new InputItem("SMPL.AGGPRO","SMPL.AGGPRO - Posto di lavoro aggiuntivo (Bundle Produzione)" ,"SMPL.AGGPRO"),
				new InputItem("SMPL.AGGFLL","SMPL.AGGFLL - Posto di lavoro aggiuntivo (Bundle Full)" ,"SMPL.AGGFLL"),
			}));
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("upgradePag2");
		}

		private void _AddActivity_TipoAttivazioneUpgradePag2(Workflow wf)
		{
			Activity a = wf.CreateActivity("upgradePag2");
			a.Title = "Quale upgrade desideri attivare?<span style='font-size:16px'>2/2</span>";
			a.TestoRiepilogo = "Upgrade da attivare:";
			a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
				new InputItem("SMPL.STM","SMPL.STM -  Personalizzazione Report di stampa","SMPL.STM"),
				new InputItem("SMPL.WOO","SMPL.WOO - Connettore Woo Commerce ","SMPL.WOO"),
				new InputItem("SMPL.QDC","SMPL.QDC - Connettore Quaderno di Campagna","SMPL.QDC"),
			}));
			a.DrawPage = _DrawPage;
			a.AllowNoChoice = true;

			Branch b1 = a.CreateBranchTo("numeroPostiDiLavoro");
			b1.Condition.IfOutputOfActivityContainsItem("upgrade", "SMPL.AGGCMM").Or.IfOutputOfActivityContainsItem("upgrade", "SMPL.AGGCMP")
				.Or.IfOutputOfActivityContainsItem("upgrade", "SMPL.AGGSRV").Or.IfOutputOfActivityContainsItem("upgrade", "SMPL.AGGSRP")
				.Or.IfOutputOfActivityContainsItem("upgrade", "SMPL.AGGPRO").Or.IfOutputOfActivityContainsItem("upgrade", "SMPL.AGGFLL");

			Branch b2 = a.CreateBranchTo("uploadFile");
		}

		private void _AddActivity_PostiDiLavoro(Workflow wf)
		{
			Activity a = wf.CreateActivity("numeroPostiDiLavoro");
			a.Title = "Quanti posti di lavoro vuoi aggiungere?";
			a.TestoRiepilogo = "Numero di posti di lavoro:";
			a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
				new InputItem("{'Key':'postiDiLavoroSMPL','Text':'Postazioni di lavoro aggiuntive','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'postiDiLavoroSMPL'}"),
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
			a.DrawPage = _DrawPage;
		}

	}
}
