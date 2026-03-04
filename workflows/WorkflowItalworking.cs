using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowItalworking : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }

        private List<string> GetActivities(Type type)
        {
            List<string> activities = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
            }

            return activities;
        }

        public WorkflowItalworking(string key, string title, Action<StateContext> drawPage) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowItalworking));

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
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
            //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'hidden','Index':0}"),
            //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
            //}));
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                 new InputItem("Demo","Demo"),
                  new InputItem("Standard", "Standard"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoGestione");
            b1.Condition.IfOutputContainsItem("Standard");

            Branch b2 = a.CreateBranchTo("tipoGestioneDEMO");
            b2.Condition.IfOutputContainsItem("Demo");
        }

        private void _AddActivity_TipoGestione(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoGestione");
            a.Title = "Quale tipologia di moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipologia di moduli da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                //new InputItem("base", "Modulo base","4508004;4550014;4550024;4550034;6008173"),
                new InputItem("base", "Modulo base","4508004"),
                new InputItem("bundle", "Bundle"),
                //new InputItem("modAggiuntivi","Moduli aggiuntivi","4550014;4550024;4550034;6008173"),
                new InputItem("modAggiuntivi","Moduli aggiuntivi"),
                new InputItem("upgrade", "Upgrade")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoModuli");
            b1.Condition.IfOutputContainsItem("base");
            Branch b2 = a.CreateBranchTo("Bundle");
            b2.Condition.IfOutputContainsItem("bundle");
            Branch b3 = a.CreateBranchTo("tipoModuliUpgrade");
            b3.Condition.IfOutputContainsItem("modAggiuntivi");
            Branch b4 = a.CreateBranchTo("postazLavoroItalWorking");
            b4.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_TipoGestioneDEMO(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoGestioneDEMO");
            a.Title = "Quale tipologia di moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipologia di moduli da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                //new InputItem("base", "Modulo base","4508004;4550014;4550024;4550034;6008173"),
                new InputItem("base", "Modulo base"),
                new InputItem("bundle", "Bundle"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoModuliDEMO");
            b1.Condition.IfOutputContainsItem("base");
            Branch b2 = a.CreateBranchTo("Bundle");
            b2.Condition.IfOutputContainsItem("bundle");
        }

        private void _AddActivity_PostazioniLavoro(Workflow wf)
        {
            Activity a = wf.CreateActivity("postazLavoroItalWorking");
            a.Title = "Vuoi aggiungere postazioni aggiuntive?";
            a.TestoRiepilogo = "Postazioni aggiuntive";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                //new InputItem("4518144", "Posto di lavoro client/accesso aggiuntivo",1),
                 new InputItem("{'Key':'postazioniItal','Text':'4518144 - Posti di lavoro client/accesso','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1, 'Tag':'4518144'}"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_TipoModuliUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModuliUpgrade");
            a.Title = "Quale tipologia di moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipologia di moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("aaModuliAggiuntivi", "Area amministrativa"),
                new InputItem("aBModuliAggiuntivi", "Area Bilancio e Controllo di Gestione"),
                new InputItem("ALCModuliAggiuntivi","Area logistica – commerciale"),
                new InputItem("verticali", "Verticali"),
                new InputItem("asModAggiuntivi", "Area statistiche"),
                new InputItem("PMI", "Area Fiscale"),
                new InputItem("tools", "Area tools"),
                new InputItem("4508224","4508224 - Generazione file XML","4508224")
                //new InputItem("moduliVari", "Moduli vari"),
                //new InputItem("Bundle", "Altre configurazioni per piccola e media azienda"),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("AAModuliAggiuntivi");
            b1.Condition.IfOutputContainsItem("aaModuliAggiuntivi");
            Branch b2 = a.CreateBranchTo("aBModuliAggiuntivi");
            b2.Condition.IfOutputContainsItem("aBModuliAggiuntivi");
            Branch b3 = a.CreateBranchTo("ALCModuliAggiuntivi");
            b3.Condition.IfOutputContainsItem("ALCModuliAggiuntivi");
            Branch b4 = a.CreateBranchTo("verticali");
            b4.Condition.IfOutputContainsItem("verticali");
            Branch b5 = a.CreateBranchTo("asModAggiuntivi");
            b5.Condition.IfOutputContainsItem("asModAggiuntivi");
            Branch b6 = a.CreateBranchTo("PMI");
            b6.Condition.IfOutputContainsItem("PMI");
            Branch b7 = a.CreateBranchTo("tools");
            b7.Condition.IfOutputContainsItem("tools");
            //Branch b8 = a.CreateBranchTo("moduliVari");
            //b7.Condition.IfOutputContainsItem("moduliVari");
            //Branch b8 = a.CreateBranchTo("Bundle");
            //b8.Condition.IfOutputContainsItem("Bundle");
            Branch b8 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_TipoModuli(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModuli");
            a.Title = "Quale tipologia di moduli vuoi attivare?";
            a.TestoRiepilogo = "Tipologia di moduli da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("aaModuliAggiuntivi", "Area amministrativa"),
                new InputItem("aBModuliAggiuntivi", "Area Bilancio e Controllo di Gestione"),
                new InputItem("ALCModuliAggiuntivi","Area logistica – commerciale"),
                new InputItem("verticali", "Verticali"),
                new InputItem("asModAggiuntivi", "Area statistiche"),
                new InputItem("PMI", "Area Fiscale"),
                new InputItem("tools", "Area tools"),
                  new InputItem("4508224","4508224 - Generazione file XML","4508224")
                //new InputItem("moduliVari", "Moduli vari"),
                //new InputItem("Bundle", "Altre configurazioni per piccola e media azienda"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("AAModuliAggiuntivi");
            b1.Condition.IfOutputContainsItem("aaModuliAggiuntivi");
            Branch b2 = a.CreateBranchTo("aBModuliAggiuntivi");
            b2.Condition.IfOutputContainsItem("aBModuliAggiuntivi");
            Branch b3 = a.CreateBranchTo("ALCModuliAggiuntivi");
            b3.Condition.IfOutputContainsItem("ALCModuliAggiuntivi");
            Branch b4 = a.CreateBranchTo("verticali");
            b4.Condition.IfOutputContainsItem("verticali");
            Branch b5 = a.CreateBranchTo("asModAggiuntivi");
            b5.Condition.IfOutputContainsItem("asModAggiuntivi");
            Branch b6 = a.CreateBranchTo("PMI");
            b6.Condition.IfOutputContainsItem("PMI");
            Branch b7 = a.CreateBranchTo("tools");
            b7.Condition.IfOutputContainsItem("tools");
            //Branch b8 = a.CreateBranchTo("moduliVari");
            //b7.Condition.IfOutputContainsItem("moduliVari");
            //Branch b8 = a.CreateBranchTo("Bundle");
            //b8.Condition.IfOutputContainsItem("Bundle");
            Branch b8 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_AAModuliAggiuntivi(Workflow wf)
        {
            Activity a = wf.CreateActivity("AAModuliAggiuntivi");
            a.Title = "Area amministrativa: quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Area amministrativa: tipologia di moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4538004","4538004 - Modulo contabile integrato","4538004"),
                new InputItem("4538074","4538074 - Gestione cespiti","4538074"),
                new InputItem("ITW.ELUSIONE","ITW.ELUSIONE - Denuncia IVA periodica e invio dati fattura attiva e passiva","ITW.ELUSIONE"),
                new InputItem("4538094","4538094 - Import prime note da Home Banking","4538094"),
                new InputItem("4508054","4508054 - Effetti","4508054"),
                new InputItem("4508064","4508064 - Scadenziario","4508064"),
                new InputItem("4508074","4508074 - Insoluti","4508074"),
                new InputItem("4508234","4508234 - Gestione Solleciti","4508234")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("aBModuliAggiuntivi");
            b1.Condition.IfOutputOfActivityContainsItem("tipoModuli", "aBModuliAggiuntivi");
            Branch b2 = a.CreateBranchTo("ALCModuliAggiuntivi");
            b2.Condition.IfOutputOfActivityContainsItem("tipoModuli", "ALCModuliAggiuntivi");
            Branch b3 = a.CreateBranchTo("verticali");
            b3.Condition.IfOutputOfActivityContainsItem("tipoModuli", "verticali");
            Branch b4 = a.CreateBranchTo("asModAggiuntivi");
            b4.Condition.IfOutputOfActivityContainsItem("tipoModuli", "asModAggiuntivi");
            Branch b5 = a.CreateBranchTo("PMI");
            b5.Condition.IfOutputOfActivityContainsItem("tipoModuli", "PMI");
            Branch b6 = a.CreateBranchTo("tools");
            b6.Condition.IfOutputOfActivityContainsItem("tipoModuli", "tools");
            //Branch b6 = a.CreateBranchTo("moduliVari");
            //b6.Condition.IfOutputOfActivityContainsItem("tipoModuli", "moduliVari");
            //Branch b7 = a.CreateBranchTo("Bundle");
            //b7.Condition.IfOutputOfActivityContainsItem("tipoModuli", "Bundle");

            Branch b7 = a.CreateBranchTo("aBModuliAggiuntivi");
            b7.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "aBModuliAggiuntivi");
            Branch b8 = a.CreateBranchTo("ALCModuliAggiuntivi");
            b8.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "ALCModuliAggiuntivi");
            Branch b9 = a.CreateBranchTo("verticali");
            b9.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "verticali");
            Branch b10 = a.CreateBranchTo("asModAggiuntivi");
            b10.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "asModAggiuntivi");
            Branch b11 = a.CreateBranchTo("PMI");
            b11.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "PMI");
            Branch b12 = a.CreateBranchTo("tools");
            b12.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "tools");

            Branch b13 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_aBModuliAggiuntivi(Workflow wf)
        {
            Activity a = wf.CreateActivity("aBModuliAggiuntivi");
            a.Title = "Area Bilancio: quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Area bilancio: tipologia di moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4538014","4538014 - Bilancio IV Direttiva CEE e Nota Integrativa","4538014"),
                //new InputItem("453814","Analisi di bilancio","453814"),
                new InputItem("4531814","4531814 - Analisi di bilancio","4531814"),
                //new InputItem("4532014","Bilancio Consolidato","4532014"),
                new InputItem("4532014","4532014 - Bilancio Consolidato","4532814"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("ALCModuliAggiuntivi");
            Branch b2 = a.CreateBranchTo("verticali");
            Branch b3 = a.CreateBranchTo("asModAggiuntivi");
            Branch b4 = a.CreateBranchTo("PMI");
            Branch b5 = a.CreateBranchTo("tools");
            //Branch b5 = a.CreateBranchTo("moduliVari");
            //Branch b6 = a.CreateBranchTo("Bundle");

            Branch b6 = a.CreateBranchTo("ALCModuliAggiuntivi");
            Branch b7 = a.CreateBranchTo("verticali");
            Branch b8 = a.CreateBranchTo("asModAggiuntivi");
            Branch b9 = a.CreateBranchTo("PMI");
            Branch b10 = a.CreateBranchTo("tools");

            b1.Condition.IfOutputOfActivityContainsItem("tipoModuli", "ALCModuliAggiuntivi");
            b2.Condition.IfOutputOfActivityContainsItem("tipoModuli", "verticali");
            b3.Condition.IfOutputOfActivityContainsItem("tipoModuli", "asModAggiuntivi");
            b4.Condition.IfOutputOfActivityContainsItem("tipoModuli", "PMI");
            b5.Condition.IfOutputOfActivityContainsItem("tipoModuli", "tools");
            b6.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "ALCModuliAggiuntivi");
            b7.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "verticali");
            b8.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "asModAggiuntivi");
            b9.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "PMI");
            b10.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "tools");

            //b5.Condition.IfOutputOfActivityContainsItem("tipoModuli", "moduliVari");
            //b6.Condition.IfOutputOfActivityContainsItem("tipoModuli", "Bundle");

            Branch b11 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ALCModuliAggiuntivi(Workflow wf)
        {
            Activity a = wf.CreateActivity("ALCModuliAggiuntivi");
            a.Title = "Area logistica: quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Area logistica: tipologia di moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4508044","4508044 - Magazzino","4508044"),
                //new InputItem("4500084","Ordini fornitori e impegni clienti","4500084"),
                //new InputItem("4500014","Vendite","4500014"),
                new InputItem("4500084","4500084 - Ordini fornitori e impegni clienti","4508084"),
                new InputItem("4500014","4500014 - Vendite","4508014"),
                new InputItem("4508094","4508094 - Agenti","4508094"),
                new InputItem("4508104","4508104 - Distinta base","4508104"),
                new InputItem("4508734","4508734 - Gestione Fornitore Remoto per Drop Shipping","4508734"),
                new InputItem("4508634","4508634 - Download Immagini da Web","4508634"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("verticali");
            Branch b2 = a.CreateBranchTo("asModAggiuntivi");
            Branch b3 = a.CreateBranchTo("PMI");
            Branch b4 = a.CreateBranchTo("tools");
            //Branch b4 = a.CreateBranchTo("moduliVari");
            //Branch b5 = a.CreateBranchTo("Bundle");
            Branch b5 = a.CreateBranchTo("verticali");
            Branch b6 = a.CreateBranchTo("asModAggiuntivi");
            Branch b7 = a.CreateBranchTo("PMI");
            Branch b8 = a.CreateBranchTo("tools");

            b1.Condition.IfOutputOfActivityContainsItem("tipoModuli", "verticali");
            b2.Condition.IfOutputOfActivityContainsItem("tipoModuli", "asModAggiuntivi");
            b3.Condition.IfOutputOfActivityContainsItem("tipoModuli", "PMI");
            b4.Condition.IfOutputOfActivityContainsItem("tipoModuli", "tools");
            //b4.Condition.IfOutputOfActivityContainsItem("tipoModuli", "moduliVari");
            //b5.Condition.IfOutputOfActivityContainsItem("tipoModuli", "Bundle");
            b5.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "verticali");
            b6.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "asModAggiuntivi");
            b7.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "PMI");
            b8.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "tools");

            Branch b9 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ALCModuliAggiuntiviDEMO(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoModuliDEMO");
            a.Title = "Attivazione Demo: quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Attivazione Demo: tipologia di moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4508004","4508004 - Italworking modulo base","4508004"),
                new InputItem("4538004","4538004 - Modulo contabile integrato","4538004")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }


        private void _AddActivity_Verticali(Workflow wf)
        {
            Activity a = wf.CreateActivity("verticali");
            a.Title = "Verticali - quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4508114","4508114 - Cauzioni","4508114"),
                new InputItem("4508134","4508134 - Vendita al banco e registratori di cassa ","4508134"),
                //new InputItem("4508124","4508124 - Terminale portatile","4508124"),
                new InputItem("4508154","4508154 - Documenti fiscali prenumerati","4508154"),
                new InputItem("4508164","4508164 - Gestione varianti","4508164"),
                new InputItem("4508174","4508174 - Gestione Lotti e Tracciabilità","4508174"),
                new InputItem("4508184","4508184 - Conai","4508184"),
                new InputItem("4508194","4508194 - Gestione contratti","4508194"),
                new InputItem("4508204","4508204 - Gestione interventi","4508204"),
                new InputItem("4508214","4508214 - Gestione fitosanitari","4508214"),
                new InputItem("4508534","4508534 - Gestione Gift Card","4508534"),
                new InputItem("4508434","4508434 - Collegamento con terminale PDA con Web App","4508434")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("asModAggiuntivi");
            Branch b2 = a.CreateBranchTo("PMI");
            Branch b3 = a.CreateBranchTo("tools");
            //Branch b3 = a.CreateBranchTo("moduliVari");
            //Branch b4 = a.CreateBranchTo("Bundle");
            Branch b4 = a.CreateBranchTo("asModAggiuntivi");
            Branch b5 = a.CreateBranchTo("PMI");
            Branch b6 = a.CreateBranchTo("tools");

            b1.Condition.IfOutputOfActivityContainsItem("tipoModuli", "asModAggiuntivi");
            b2.Condition.IfOutputOfActivityContainsItem("tipoModuli", "PMI");
            b3.Condition.IfOutputOfActivityContainsItem("tipoModuli", "tools");
            //b3.Condition.IfOutputOfActivityContainsItem("tipoModuli", "moduliVari");
            //b4.Condition.IfOutputOfActivityContainsItem("tipoModuli", "Bundle");
            b4.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "asModAggiuntivi");
            b5.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "PMI");
            b6.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "tools");

            Branch b7 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ASModuliAggiuntivi(Workflow wf)
        {
            Activity a = wf.CreateActivity("asModAggiuntivi");
            a.Title = "Area statistiche: quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Area statistiche - moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4548014","4548014 - Analisi Economica articoli, clienti e fornitori","4548014"),
                new InputItem("4548024","4548024 - Indici di rotazione","4548024"),
                new InputItem("4548034","4548034 - Indici di variazione","4548034"),
                new InputItem("4508334","4508334 - Statistica (Analisi e Generazione Scorte Dinamiche)","4508334")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("PMI");
            //Branch b2 = a.CreateBranchTo("moduliVari");
            Branch b2 = a.CreateBranchTo("PMI");
            Branch b3 = a.CreateBranchTo("tools");
            Branch b4 = a.CreateBranchTo("tools");

            b1.Condition.IfOutputOfActivityContainsItem("tipoModuli", "PMI");
            b2.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "PMI");
            b3.Condition.IfOutputOfActivityContainsItem("tipoModuli", "tools");
            b4.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "tools");

            Branch b5 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_GAPMI(Workflow wf)
        {
            Activity a = wf.CreateActivity("PMI");
            a.Title = "Area Fiscale - quali moduli vuoi attivare?";
            a.TestoRiepilogo = "Gestionale aziendale per piccole e medie imprese - moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4538064","4538064 - Iva dichiarazione annuale","4538064"),
                new InputItem("4538054","4538054 - 770 dichiarazione dei sostituti di imposta","4538054"),
                new InputItem("4538044","4538044 - Tutto adempimenti fiscali","4538044")

            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("moduliVari");
            Branch b2 = a.CreateBranchTo("Bundle");

            Branch b3 = a.CreateBranchTo("tools");
            Branch b4 = a.CreateBranchTo("tools");

            //b1.Condition.IfOutputOfActivityContainsItem("tipoModuli", "moduliVari");
            b2.Condition.IfOutputOfActivityContainsItem("tipoModuli", "Bundle");
            b3.Condition.IfOutputOfActivityContainsItem("tipoModuli", "tools");
            b4.Condition.IfOutputOfActivityContainsItem("tipoModuliUpgrade", "tools");

            Branch b5 = a.CreateBranchTo("uploadFile");

        }

        private void _AddActivity_AreaTools(Workflow wf)
        {
            Activity a = wf.CreateActivity("tools");
            a.Title = "Area Tools - vuoi attivare il seguente modulo?";
            a.TestoRiepilogo = "Area Tools - modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                //new InputItem("4208004","4208004 - Tools Modifica Report (S.A.P.+S.A.T.)","4208004"),
                new InputItem("4208004","4208004 - Tools Modifica Report (S.A.P.+S.A.T.)","4208004"),
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("moduliVari");
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_ModuliVari(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("moduliVari");
        //    a.Title = "Moduli compresi in tutte le configurazioni:";
        //    a.TestoRiepilogo = "Moduli compresi:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {               
        //        new InputItem("4550014","4550014 - Collegamento con lo studio","4550014",true, true),
        //        new InputItem("4550024","4550024 - Importazione dati","4550024",true, true),
        //        new InputItem("4550034","4550034 - Stampa laser di tutti i modelli gestiti e generazione del file telematico ove previsto","4550034", true, true),
        //        new InputItem("6008173","6008173 - Invio e-mail massivo e mail merge","6008173", true, true),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    //Branch b1 = a.CreateBranchTo("Bundle");
        //    //b1.Condition.IfOutputOfActivityContainsItem("tipoModuli", "PMI");

        //    Branch b2 = a.CreateBranchTo("uploadFile");
        //}


        private void _AddActivity_ModuliVariPMI(Workflow wf)
        {
            Activity a = wf.CreateActivity("Bundle");
            a.Title = "Quale bundle vuoi attivare?";
            a.TestoRiepilogo = "Bundle da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("4521804","4521804 - Bundle mini","4521804"),
                new InputItem("4522804","4522804 - Bundle Mini plus","4522804"),
                new InputItem("4528004","4528004 - Bundle entry","4528004"),
                new InputItem("4528014","4528014 - Bundle entry plus","4528014"),
                new InputItem("4528024","4528024 - Bundle advanced","4528024")
            }));
            a.DrawPage = _DrawPage;

            //Branch b1 = a.CreateBranchTo("moduliTutti");
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_ModuliPerTutti(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("moduliTutti");
        //    a.Title = "Moduli compresi in tutte le configurazioni";
        //    a.TestoRiepilogo = "Moduli compresi in tutte le configurazioni:";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("4550014","4550014 - Collegamento con lo Studio","4550014"),
        //        new InputItem("4550024","4550024 - Importazione dati","4550024"),
        //        new InputItem("4550034","4550034 - Stampa laser di tutti i modelli gestiti e generazione del file telematico ove previsto","4550034"),
        //        new InputItem("6008173","6008173 - Invio e-mail massivo & mail-merge","6008173"),
        //    }));
        //    a.DrawPage = _DrawPage;

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
            a.DrawPage = _DrawPage;
        }

    }
}
