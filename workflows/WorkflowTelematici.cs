using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowTelematici : Workflow
    {
        //public bool possiedeContratto { get; set; }  //A.Lucchi(03/05/2021)

        //public bool possiedeConsulentiLavoro { get; set; } //A.Lucchi (17/05/2021)
        //public bool possiedeRateizzazioniEScadenze { get; set; } //A.Lucchi (17/05/2021)

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

        public WorkflowTelematici(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto, bool possiedeConsulentiLavoro, bool possiedeRateizzazioniEScadenze*/) : base(key, title)  //A.Lucchi(27/04/2021)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowTelematici));

            //this.possiedeContratto = possiedeContratto;  //A.Lucchi(03/05/2021)
            //this.possiedeConsulentiLavoro = possiedeConsulentiLavoro;
            //this.possiedeRateizzazioniEScadenze = possiedeRateizzazioniEScadenze;

            foreach (string s in methods)
            {
                MethodInfo m = this.GetType().GetMethod(s, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        //private void _AddActivity_TipoLicenza(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("lic");
        //    a.Title = "Che tipo di licenza desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("demo", "Demo"),
        //        new InputItem("stan", "Standard")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("ver");
        //    b1.Condition.IfOutputContainsItem("demo");

        //    Branch b2 = a.CreateBranchTo("ver");
        //    b2.Condition.IfOutputContainsItem("stan");
        //}

        private void _AddActivity_TipoLicenza(Workflow wf)
        {
            Activity a = wf.CreateActivity("lic");
            a.Title = "Che tipo di licenza desideri attivare?";
            a.TestoRiepilogo = "Tipo di licenza da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
            //    //new InputItem("demo", "Demo"),
            //    //new InputItem("stan", "Standard"),
            //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'hidden','Index':0}"),
            //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
            //}));

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                 new InputItem("Demo","Demo"),
                  new InputItem("Standard", "Standard"),
            }));

            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("ver");

            Branch b1 = null;
            //if (possiedeContratto)
            //    b1 = a.CreateBranchTo("tipoGestioneUpgrade");
            //else
            //    b1 = a.CreateBranchTo("tipoGestioneNuovo");
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
                new InputItem("6018053", "6018053 - Modulo Base fino a 500 anagrafiche","6018053")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }
        //private void _AddActivity_TipoGestioneNuovo(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoGestioneNuovo");
        //    a.Title = "Che tipo di modulo desideri attivare?";
        //    a.TestoRiepilogo = "Tipo di modulo da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("gestioni", "Gestioni"),
        //        new InputItem("bundle", "Bundle"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("desk.tel");
        //    b1.Condition.IfOutputContainsItem("gestioni");

        //    Branch b2 = a.CreateBranchTo("dsk.bundle");
        //    b2.Condition.IfOutputContainsItem("bundle");
        //}

        //private void _AddActivity_TipoGestioneUpgrade(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoGestioneUpgrade");
        //    a.Title = "Che tipo di modulo desideri attivare?";
        //    a.TestoRiepilogo = "Tipo di modulo da attivare:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("gestioni", "Gestioni"),
        //        new InputItem("upgrade", "Upgrade"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("desk.tel");
        //    b1.Condition.IfOutputContainsItem("gestioni");

        //    Branch b2 = a.CreateBranchTo("desk.upgrade");
        //    b2.Condition.IfOutputContainsItem("upgrade");
        //}

        //private void _AddActivity_Versione(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("ver");
        //    a.Title = "Quale versione desideri attivare?";

        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("tok", "TuttOk Desktop"),
        //        new InputItem("etel", "Espando Telematici Cloud")  //A.Lucchi(27/04/2021)
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = null;  //A.Lucchi(27/04/2021)
        //    Branch b2 = null;

        //    //if (possiedeContrattoDsk)
        //    //{
        //    //    b1 = a.CreateBranchTo("desk.upgrade");
        //    //    b1.Condition.IfOutputContainsItem("tok");
        //    //}
        //    //else
        //    //{
        //    //    b1 = a.CreateBranchTo("desk.tel");
        //    //    b1.Condition.IfOutputContainsItem("tok");
        //    //}

        //    //if (possiedeContrattoCloud)
        //    //{
        //    //    b2 = a.CreateBranchTo("cloud.upgrade");
        //    //    b2.Condition.IfOutputContainsItem("etel");
        //    //}
        //    //else
        //    //{
        //    //    b2 = a.CreateBranchTo("cloud.tel");
        //    //    b2.Condition.IfOutputContainsItem("etel");
        //    //}
        //}

        //private void _AddActivity_TipoContratto(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoContratto");
        //    a.Title = "Quale versione desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("nContr", "Nuovo contratto"),
        //        new InputItem("upgrade", "Upgrade di un contratto esistente")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("desk.tel");
        //    b1.Condition.IfOutputOfActivityContainsItem("ver", "tok").And.IfOutputContainsItem("nContr");

        //    Branch b2 = a.CreateBranchTo("cloud.tel");
        //    b2.Condition.IfOutputOfActivityContainsItem("ver", "etel").And.IfOutputContainsItem("nContr");

        //    Branch b3 = a.CreateBranchTo("desk.upgrade");
        //    b3.Condition.IfOutputOfActivityContainsItem("ver", "tok").And.IfOutputContainsItem("upgrade");

        //    Branch b4 = a.CreateBranchTo("cloud.upgrade");
        //    b4.Condition.IfOutputOfActivityContainsItem("ver", "etel").And.IfOutputContainsItem("upgrade");
        //}

        private void _AddActivity_TelematiciDesktop(Workflow wf)
        {
            Activity a = wf.CreateActivity("desk.tel");
            a.Title = "Che moduli desideri attivare? <span style='font-size:20px'>1 di 2</span>";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("entra", "6518003 - Telematici Entratel", "6518003"),
                new InputItem("inps", "6538003 - Telematici INPS", "6538003"),
                new InputItem("cciaa", "6008183 - Telematici CCIAA", "6008183"),
                new InputItem("730", "6008203 - Spese per 730 precompilato", "6008203"),
                new InputItem("anetie", "6548003 - Adempimenti Antielusione", "6548003"),
                new InputItem("ascrive", "6008333 - Agenzia scrive", "6008333"),
                new InputItem("6008223", "6008223 - Avvisi Telematici","6008223"),
                new InputItem("alle", "6558003 - Archiviazione allegati", "6558003")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("desk.modentra");
            //b1.Condition.IfOutputContainsItem("entra");

            //Branch b2 = a.CreateBranchTo("desk.modinps");
            //b2.Condition.IfOutputContainsItem("inps");

            //Branch b3 = a.CreateBranchTo("Anagrafiche");
            //Branch b3 = a.CreateBranchTo("uploadFile");  //A.Lucchi(27/04/2021)
        }

        //private void _AddActivity_TelematiciCloud(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("cloud.tel");
        //    a.Title = "Vuoi creare un nuovo contratto?";  //A.Lucchi(27/04/2021)
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("entra", "Telematici Entratel","6518009"),
        //        new InputItem("cciaa", "Telematici CCIAA","6008199"),
        //        new InputItem("730", "Spese per 730 precompilato","6008209"),
        //        new InputItem("antie", "Adeguamenti Antielusione","6548009"), //Check, OK solo su sviluppo
        //        new InputItem("ascrive", "Agenzia scrive","6008339")  //C'è solo SD e ITW -> Da listino, cambiare la visibilità
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("cloud.modentra");
        //    b1.Condition.IfOutputContainsItem("entra");

        //    //Branch b2 = a.CreateBranchTo("Anagrafiche");
        //    Branch b2 = a.CreateBranchTo("uploadFile");
        //}

        private void _AddActivity_TelematiciDskUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("desk.upgrade");
            a.Title = "Vuoi effettuare l'upgrade del contratto?";
            a.TestoRiepilogo = "Upgrade del contratto:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
            //    new InputItem("500a", "6508503 - Modulo base per gestione ulteriore anagrafiche soggetto (ogni 500)", 500, "6508503"),
            //    new InputItem("200a", "6508504 - Ulteriore anagrafiche soggetto cliente al bundle consulenti del lavoro<br/>(ogni 200)", 200, "6508504"),
            //    new InputItem("6018233", "6018233 - Gestione completa Agenzia Scrive – Civis per i licenziatari del modulo<br/>'Rateizzazione e scadenze'", "6018233"),
            //    new InputItem("6008143", "6008143 - Attivazione sedi", "6008143"),
            //}));

            InputItem iiMBUlterioriAnagraficheSoggetto = new InputItem("{'Key':'qtaUpgrade','Text':'6508503 - Modulo base per gestione ulteriore anagrafiche soggetto (ogni 500)','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6508503','Style':'visible','Index':0}");
            InputItem iiUMConsulentiLavoro = new InputItem("{'Key':'qtaUpgrade','Text':'6508504 - Ulteriore anagrafiche soggetto cliente al bundle consulenti del lavoro (ogni 200)','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6508504','Style':'hidden','Index':1}");
            InputItem iiUMASCivis = new InputItem("{'Key':'qtaUpgrade','Text':'6018233 - Gestione completa Agenzia Scrive – Civis per i licenziatari del modulo Rateizzazione e scadenze','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6018233','Style':'hidden','Index':2}");
            InputItem iiAttSedi = new InputItem("{'Key':'qtaUpgrade','Text':'6008143 - Attivazione sedi','MinValue':1,'MaxValue':10,'DataType':'radioText', 'Tag':'6008143','Style':'Visible','Index':3}");

            
            int max = 2;
            max = 4;
            //if (this.possiedeConsulentiLavoro) max++;
            //if (this.possiedeRateizzazioniEScadenze) max++;

            InputItem[] itemUpgrade = new InputItem[max];
            int i = 0;

            itemUpgrade[i] = iiMBUlterioriAnagraficheSoggetto;
            i++;

            //if (this.possiedeConsulentiLavoro)
            //{
                itemUpgrade[i] = iiUMConsulentiLavoro;
                i++;
            //}

            //if (this.possiedeRateizzazioniEScadenze)
            //{
                itemUpgrade[i] = iiUMASCivis;
                i++;
            //}

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
                new InputItem("cassf", "6008153 - Cassetto fiscale", "6008153"),
                new InputItem("cassPrev", "6008193 - Cassetto Previdenziale", "6008193"),
                new InputItem("cat", "6008123 - Dati catastali online", "6008123"),
                new InputItem("rate", "6008213 - Rateizzazioni e scadenze", "6008213"),
                new InputItem("firma", "6008133 - Firma autografa automatica", "6008133"),
                //new InputItem("alle", "6558003 - Archiviazione allegati", "6558003"),
                new InputItem("mail", "6008173 - Invio e-mail massivo e mail merge", "6008173"),
                new InputItem("equi", "6568003 - Modulo Equitalia", "6568003"),
                new InputItem("CPB.TOK", "CPB.TOK - Recupero CBP comunicazioni AdE", "CPB.TOK")
            }));
            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("desk.modinps");
            //b1.Condition.IfOutputOfActivityContainsItem("desk.tel", "inps");

            //Branch b2 = a.CreateBranchTo("Anagrafiche");
            Branch b2 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_ModuliEntratelCloud(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("cloud.modentra");
        //    a.Title = "Quali moduli Entratel desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("cassf", "Cassetto fiscale","6008159", true),
        //        new InputItem("cat", "Dati catastali online","6008129"),
        //        new InputItem("rate", "Rateizzazioni e scadenze", "6008219"),
        //        new InputItem("firma", "Firma autografa automatica", "6008139"),
        //        new InputItem("equi", "Modulo Equitalia","6568009")
        //    }));
        //    a.AllowNoChoice = true;
        //    a.DrawPage = _DrawPage;

        //    //Branch b2 = a.CreateBranchTo("AnagraficheCloud");
        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

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

            //Branch b1 = a.CreateBranchTo("dsk.bundle");
            //b1.Condition.IfOutputContainsItem("bundle");

            //Branch b2 = a.CreateBranchTo("Anagrafiche");
            Branch b2 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_Bundle(Workflow wf)
        {
            Activity a = wf.CreateActivity("dsk.bundle");
            a.Title = "Quali bundle desideri acquistare?";
            a.TestoRiepilogo = "Bundle da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
            {
                new InputItem("6000014","6000014 - Bundle Tuttok per Enti pubblici", "6008014"),
                new InputItem("6000024", "6000024 - Bundle Tuttok per Consulenti del lavoro", "6008024" ),
                //new InputItem("29780031", "29780031 - Bundle Tuttok per Amministratori di condominio", "6000024;6518003;6538003")
            }));

            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("Anagrafiche");
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