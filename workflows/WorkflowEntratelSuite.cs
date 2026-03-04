using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowEntratelSuite : Workflow
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

        public WorkflowEntratelSuite(string key, string title, Action<StateContext> drawPage) : base(key, title)  //A.Lucchi(27/04/2021)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowEntratelSuite));

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        private void _AddActivity_TipoLicenza(Workflow wf)
        {
            Activity a = wf.CreateActivity("lic");
            a.Title = "Che tipo di licenza desideri attivare?";
            a.TestoRiepilogo = "Tipo di licenza da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                 new InputItem("Demo","Demo"),
                  new InputItem("Standard", "Standard"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = null;
            b1 = a.CreateBranchTo("tipoGestione");
            b1.Condition.IfOutputContainsItem("Standard");

            Branch b2 = a.CreateBranchTo("tipoGestioneDEMO");
            b2.Condition.IfOutputContainsItem("Demo");
        }

        private void _AddActivity_TipoGestioneNuovo(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoGestione");
            a.Title = "Che tipo di modulo desideri attivare?";
            a.TestoRiepilogo = "Tipo di modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("gestioni", "Gestioni"),
                new InputItem("bundle", "Bundle"),
                 new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("desk.tel");
            b1.Condition.IfOutputContainsItem("gestioni");

            Branch b2 = a.CreateBranchTo("dsk.bundle");
            b2.Condition.IfOutputContainsItem("bundle");

            Branch b3 = a.CreateBranchTo("desk.upgrade");
            b3.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_TipoGestioneDEMO(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoGestioneDEMO");
            a.Title = "Che tipo di modulo desideri attivare?";
            a.TestoRiepilogo = "Tipo di modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("6018053.ES", "6018053.ES - Modulo Base fino a 500 anagrafiche","6018053.ES")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_TelematiciDesktop(Workflow wf)
        {
            Activity a = wf.CreateActivity("desk.tel");
            a.Title = "Che moduli desideri attivare? <span style='font-size:20px'>1 di 2</span>";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("6518003.ES", "6518003.ES - Telematici Entratel", "6518003.ES"),
                new InputItem("inps", "6530003 - Telematici INPS", "6530003"),
                new InputItem("cciaa", "6000183 - Telematici C.C.I.A.A. (Telemaco/ComUnica)", "6000183"),
                new InputItem("730", "6530033 - Spese per 730 precompilato", "6530033"),
                new InputItem("6548003.ES", "6548003.ES - Adempimenti Antielusione", "6548003.ES"),
                new InputItem("ascrive", "6028333 - Agenzia Scrive - Civis", "6028333"),
                new InputItem("6008223.ES", "6008223.ES - Avvisi telematici", "6008223.ES"),
				new InputItem("alle", "6550003 Archiviazione Allegati", "6550003"),
			}));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("desk.modentra");
        }

        private void _AddActivity_TelematiciDskUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("desk.upgrade");
            a.Title = "Vuoi effettuare l'upgrade del contratto?";
            a.TestoRiepilogo = "Upgrade del contratto:";

            InputItem iiMBUlterioriAnagraficheSoggetto = new InputItem("{'Key':'qtaUpgrade','Text':'6500503 - Modulo base per gestione ulteriore anagrafiche soggetto (ogni 500)','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6500503','Style':'visible','Index':0}");
            InputItem iiUMConsulentiLavoro = new InputItem("{'Key':'qtaUpgrade','Text':'6500504 - Ulteriore anagrafiche soggetto cliente al bundle consulenti del lavoro (ogni 200)','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6500504','Style':'hidden','Index':1}");
            InputItem iiUMASCivis = new InputItem("{'Key':'qtaUpgrade','Text':'6018233.ES - Upgrade alla gestione completa “Agenzia Scrive - Civis” per i licenziatari del modulo “Rateizzazioni e scadenze”','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6018233.ES','Style':'hidden','Index':2}");
            InputItem iiAttSedi = new InputItem("{'Key':'qtaUpgrade','Text':'6000143 - Attivazione sedi','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6000143','Style':'Visible','Index':3}");


            int max = 2;
            max = 4;

            InputItem[] itemUpgrade = new InputItem[max];
            int i = 0;

            itemUpgrade[i] = iiMBUlterioriAnagraficheSoggetto;
            i++;

            itemUpgrade[i] = iiUMConsulentiLavoro;
            i++;

            itemUpgrade[i] = iiUMASCivis;
            i++;

            itemUpgrade[i] = iiAttSedi;

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(itemUpgrade));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }


        //private void _AddActivity_TelematiciCloudUpgrade(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("cloud.upgrade");
        //    a.Title = "Vuoi effettuare l'upgrade del contratto?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("500a", "Modulo base per gestione ulteriore anagrafiche soggetto (ogni 500)", 500, "6508503"),
        //       // new InputItem("6018233", "Gestione completa Agenzia Scrive – Civis per i licenziatari del modulo 'Rateizzazione e scadenze'", "6008339"),
        //        new InputItem("6008169", "Attivazione sedi", "6008169"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        private void _AddActivity_ModuliEntratelDesktop(Workflow wf)
        {
            Activity a = wf.CreateActivity("desk.modentra");
            a.Title = "Quali moduli desideri attivare? <span style='font-size:20px'>2 di 2</span>";
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("cassf", "6000153 Cassetto Fiscale", "6000153"),
                new InputItem("cassPrev", "6000193 Cassetto Previdenziale", "6000193"),
                new InputItem("cat", "6000123 Dati Catastali On-Line", "6000123"),
                new InputItem("rate", "6528003T Rateizzazioni e scadenze", "6528003T"),
                new InputItem("firma", "6000133 Firma Autografa Automatica", "6000133"),
                //new InputItem("alle", "6550003 Archiviazione Allegati", "6550003"),
                new InputItem("mail", "6000173 Invio e-mail massivo & mail-merge", "6000173"),
                new InputItem("equi", "653013.ES Modulo Agenzia Riscossione - AER", "653013.ES"),
				new InputItem("CPB.ES", "CPB.ES - Recupero CBP comunicazioni AdE", "CPB.ES")
			}));
            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            Branch b2 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuliInps(Workflow wf)
        {
            Activity a = wf.CreateActivity("desk.modinps");
            a.Title = "Quali moduli INPS desideri attivare?";
            a.TestoRiepilogo = "Moduli INPS da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("cassp", "6008193 - Cassetto previdenziale", "6008193"),
                //new InputItem("bundle", "Bundle")
            }));
            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            Branch b2 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_Bundle(Workflow wf)
        {
            Activity a = wf.CreateActivity("dsk.bundle");
            a.Title = "Quali bundle desideri acquistare?";
            a.TestoRiepilogo = "Bundle da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
            {
                new InputItem("6310023","6310023 - Bundle Entratel Suite per Consulenti del Lavoro", "6310023"),
                new InputItem("6310063", "6310063 - Modulo base (500anagrafiche + Telematici Entratel)", "6310063" ),
                new InputItem("6548063", "6548063 - Modulo base (500 anagrafiche) + Antielusione", "6548063" ),
                new InputItem("6310053", "6310053 - Modulo base (500 anagrafiche) + Entratel + Cassetto Fiscale", "6310053" )
            }));

            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_Anagrafiche(Workflow wf)
        {
            Activity a = wf.CreateActivity("Anagrafiche");
            a.Title = "Quante anagrafiche desideri attivare?";
            a.TestoRiepilogo = "Anagrafiche da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Numerical, new List<InputItem>(new InputItem[] {
                new InputItem("500a", "6508503 - 500 anagrafiche", 500, "6508503"),
                new InputItem("200a", "6508504 - 200 anagrafiche", 200, "6508504")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchToSummary();
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_AnagraficheCloud(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("AnagraficheCloud");
        //    a.Title = "Quante anagrafiche desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Numerical, new List<InputItem>(new InputItem[] {
        //        new InputItem("500a", "500 anagrafiche", 500, "6008019"),  //Manca
        //        //new InputItem("200a", "200 anagrafiche", 200)   //Manca
        //    }));
        //    a.DrawPage = _DrawPage;

        //    //Branch b1 = a.CreateBranchToSummary();
        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

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