using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowEspando : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }
        //private bool possiedeContratto { get; set; }
        private List<string> GetActivities(Type type)
        {
            List<string> activities = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) activities.Add(method.Name);
            }

            return activities;
        }

        public WorkflowEspando(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto*/) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowEspando));

            //this.possiedeContratto = possiedeContratto;

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

            //Branch b1 = a.CreateBranchTo("esDsk");

            //Branch b1 = null;
            //if (possiedeContratto)
            //{
            //    b1 = a.CreateBranchTo("modAggDsk");
            //}
            //else
            //{
            //b1 = a.CreateBranchTo("esDsk");
            //}
            Branch b1 = a.CreateBranchTo("tipoAttivazione");
            b1.Condition.IfOutputContainsItem("Standard");

            Branch b2 = a.CreateBranchTo("esDsk");
            b2.Condition.IfOutputContainsItem("Demo");
        }

        //private void _AddActivity_Versione(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("ver");
        //    a.Title = "Quale versione desideri attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("esd", "Espando studio desktop"),
        //        new InputItem("esc", "Espando studio cloud"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("esDsk");
        //    //Branch b1 = a.CreateBranchTo("tipoContrattoDsk");
        //    b1.Condition.IfOutputContainsItem("esd");

        //    Branch b2 = a.CreateBranchTo("esCloud");
        //    //Branch b2 = a.CreateBranchTo("tipoContrattoCloud");
        //    b2.Condition.IfOutputContainsItem("esc");
        //}

        //private void _AddActivity_TipoContrattoDesktop(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoContrattoDsk");
        //    a.Title = "Che tipo di contratto vuoi attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //       new InputItem("nContr", "Nuovo contratto"),
        //       new InputItem("upgrade", "Upgrade"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("esDsk");
        //    b1.Condition.IfOutputContainsItem("nContr");
        //}

        private void _AddActivity_TipoAttivazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoAttivazione");
            a.Title = "Che tipo attivazione desideri effettuare?";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Tipo di attivazione:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("nuovoContratto", "Nuova attivazione"),
               new InputItem("upgrade", "Upgrade")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("esDsk");
            b1.Condition.IfOutputContainsItem("nuovoContratto");

            Branch b2 = a.CreateBranchTo("modAggDskUpgrade");
            b2.Condition.IfOutputContainsItem("upgrade");
        }


        private void _AddActivity_TipoESDesktop(Workflow wf)
        {
            Activity a = wf.CreateActivity("esDsk");
            a.Title = "Che tipo contratto desideri attivare?";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Tipo di contratto da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("esb", "Espando Studio Base","9208039;2978002"),
               new InputItem("ese", "Espando Studio Estesa","9208039;2978002"),
               new InputItem("esl", "Espando Studio Light","2978002"),
               new InputItem("esm", "Espando Studio Modulare")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("eseDsk");
            b1.Condition.IfOutputContainsItem("ese");

            Branch b2 = a.CreateBranchTo("esbDsk");
            b2.Condition.IfOutputContainsItem("esb");

            Branch b3 = a.CreateBranchTo("eslDsk");
            b3.Condition.IfOutputContainsItem("esl");

            Branch b4 = a.CreateBranchTo("esmDsk");
            b4.Condition.IfOutputContainsItem("esm");
        }

        private void _AddActivity_ESEstesaDsk(Workflow wf)
        {
            Activity a = wf.CreateActivity("eseDsk");
            a.Title = "Quale configurazione desideri attivare?";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Configurazione da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
            //     new InputItem("2208003","2208003 - Espando Studio Estesa - Workstation","2208003"),
            //     new InputItem("2208023","2208023 - Espando Studio Estesa - fino a 2 client/accessi","2208023"),
            //     new InputItem("2208033","2208033 - Espando Studio Estesa - fino a 3 client/accessi","2208033"),
            //     new InputItem("2208043","2208043 - Espando Studio Estesa - fino a 4 client/accessi","2208043"),
            //     new InputItem("2208053","2208053 - Espando Studio Estesa - fino a 5 client/accessi","2208053"),
            //     new InputItem("2208063","2208063 - Espando Studio Estesa - fino a 6 client/accessi","2208063"),
            //     new InputItem("2208073","2208073 - Espando Studio Estesa - fino a 7 client/accessi","2208073"),
            //     new InputItem("2208083","2208083 - Espando Studio Estesa - fino a 8 client/accessi","2208083"),
            //     new InputItem("2208093","2208093 - Espando Studio Estesa - fino a 9 client/accessi","2208093"),
            //     new InputItem("2208103","2208103 - Espando Studio Estesa - fino a 10 client/accessi","2208103"),
            //     new InputItem("2208113","2208113 - Espando Studio Estesa - fino a 11 client/accessi","2208113"),
            //     new InputItem("2208123","2208123 - Espando Studio Estesa - fino a 12 client/accessi","2208123"),
            //     new InputItem("2208133","2208133 - Espando Studio Estesa - fino a 13 client/accessi","2208133"),
            //     new InputItem("2208143","2208143 - Espando Studio Estesa - fino a 14 client/accessi","2208143"),
            //     new InputItem("2208153","2208153 - Espando Studio Estesa - fino a 15 client/accessi","2208153"),
            //}));

            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[]{
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208003 - Espando Studio Estesa - Workstation','DataType':'radioText', 'Tag':'2208003','Style':'hidden','Index':0}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208023 - Espando Studio Estesa - fino a 2 client/accessi','DataType':'radioText', 'Tag':'2208023','Style':'hidden','Index':1}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208033 - Espando Studio Estesa - fino a 3 client/accessi','DataType':'radioText', 'Tag':'2208033','Style':'hidden','Index':2}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208043 - Espando Studio Estesa - fino a 4 client/accessi','DataType':'radioText', 'Tag':'2208043','Style':'hidden','Index':3}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208053 - Espando Studio Estesa - fino a 5 client/accessi','DataType':'radioText', 'Tag':'2208053','Style':'hidden','Index':4}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208063 - Espando Studio Estesa - fino a 6 client/accessi','DataType':'radioText', 'Tag':'2208063','Style':'hidden','Index':5}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208073 - Espando Studio Estesa - fino a 7 client/accessi','DataType':'radioText', 'Tag':'2208073','Style':'hidden','Index':6}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208083 - Espando Studio Estesa - fino a 8 client/accessi','DataType':'radioText', 'Tag':'2208083','Style':'hidden','Index':7}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208093 - Espando Studio Estesa - fino a 9 client/accessi','DataType':'radioText', 'Tag':'2208093','Style':'hidden','Index':8}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208103 - Espando Studio Estesa - fino a 10 client/accessi','DataType':'radioText', 'Tag':'2208103','Style':'hidden','Index':9}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208113 - Espando Studio Estesa - fino a 11 client/accessi','DataType':'radioText', 'Tag':'2208113','Style':'hidden','Index':10}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208123 - Espando Studio Estesa - fino a 12 client/accessi','DataType':'radioText', 'Tag':'2208123','Style':'hidden','Index':11}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208133 - Espando Studio Estesa - fino a 13 client/accessi','DataType':'radioText', 'Tag':'2208133','Style':'hidden','Index':12}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208143 - Espando Studio Estesa - fino a 14 client/accessi','DataType':'radioText', 'Tag':'2208143','Style':'hidden','Index':13}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208153 - Espando Studio Estesa - fino a 15 client/accessi','DataType':'radioText', 'Tag':'2208153','Style':'hidden','Index':14}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2208144 - Espando Studio Estesa - Oltre 15 ogni client/accesso aggiuntivo','DataType':'radioText', 'Tag':'2208144','Style':'visible','Index':16}"),
            //}));

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'postazioniEspando','Text':'Espando Studio Estesa client/accessi','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xEstesa'}"),
                new InputItem("{'Key':'postazioniEspando','Text':'Espando Studio Estesa accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xEstesa'}"),
                //new InputItem("{'Key':'postazioniEspando','Text':'9308519 - 1 gestione privacy con 5 incaricati + Check Security 1 interrogazione','DataType':'integer','MinValue':1,'MaxValue':1,'DefaultValue':1,'Tag':'9308519'}"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggDsk");
            b1.Condition.IfOutputOfActivityContainsItem("lic", "Standard");

            Branch b2 = a.CreateBranchTo("uploadFile");
            b2.Condition.IfOutputOfActivityContainsItem("lic", "Demo");
        }

        private void _AddActivity_ESBaseDsk(Workflow wf)
        {
            Activity a = wf.CreateActivity("esbDsk");
            a.Title = "Quale tipo di configurazione desideri attivare?";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Configurazione da attivare:";
            //a.Description = "Breve descrizione...";
            //a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
            //    new InputItem("2108003","2108003 - Espando Studio Base - Workstation","2108003"),
            //    new InputItem("2108023","2108023 - Espando Studio Base - fino a 2 client/accessi","2108023"),
            //    new InputItem("2108033","2108033 - Espando Studio Base - fino a 3 client/accessi","2108033"),
            //    new InputItem("2108043","2108043 - Espando Studio Base - fino a 4 client/accessi","2108043"),
            //    new InputItem("2108053","2108053 - Espando Studio Base - fino a 5 client/accessi","2108053"),
            //    new InputItem("2108063","2108063 - Espando Studio Base - fino a 6 client/accessi","2108063"),
            //    new InputItem("2108073","2108073 - Espando Studio Base - fino a 7 client/accessi","2108073"),
            //    new InputItem("2108083","2108083 - Espando Studio Base - fino a 8 client/accessi","2108083"),
            //    new InputItem("2108093","2108093 - Espando Studio Base - fino a 9 client/accessi", "2108093"),
            //    new InputItem("2108103","2108103 - Espando Studio Base - fino a 10 client/accessi","2108103"),
            //    new InputItem("2108113","2108113 - Espando Studio Base - fino a 11 client/accessi","2108113"),
            //    new InputItem("2108123","2108123 - Espando Studio Base - fino a 12 client/accessi","2108123"),
            //    new InputItem("2108133","2108133 - Espando Studio Base - fino a 13 client/accessi","2108133"),
            //    new InputItem("2108143","2108143 - Espando Studio Base - fino a 14 client/accessi","2108143"),
            //    new InputItem("2108153","2108153 - Espando Studio Base - fino a 15 client/accessi","2108153"),
            //    new InputItem("2008058","2008058 - Laser Espando Studio Base 50 Soggetti ","2008058")
            //}));


            //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[]{
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108003 - Espando Studio Base - Workstation','DataType':'radioText', 'Tag':'2108003','Style':'hidden','Index':0}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108023 - Espando Studio Base - fino a 2 client/accessi','DataType':'radioText',  'Tag':'2108023','Style':'hidden','Index':1}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108033 - Espando Studio Base - fino a 3 client/accessi','DataType':'radioText',  'Tag':'2108033','Style':'hidden','Index':2}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108043 - Espando Studio Base - fino a 4 client/accessi','DataType':'radioText',  'Tag':'2108043','Style':'hidden','Index':3}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108053 - Espando Studio Base - fino a 5 client/accessi','DataType':'radioText',  'Tag':'2108053','Style':'hidden','Index':4}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108063 - Espando Studio Base - fino a 6 client/accessi','DataType':'radioText',  'Tag':'2108063','Style':'hidden','Index':5}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108073 - Espando Studio Base - fino a 7 client/accessi','DataType':'radioText',  'Tag':'2108073','Style':'hidden','Index':6}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108083 - Espando Studio Base - fino a 8 client/accessi','DataType':'radioText',  'Tag':'2108083','Style':'hidden','Index':7}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108093 - Espando Studio Base - fino a 9 client/accessi','DataType':'radioText',  'Tag':'2108093','Style':'hidden','Index':8}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108103 - Espando Studio Base - fino a 10 client/accessi','DataType':'radioText', 'Tag':'2108103','Style':'hidden','Index':9}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108113 - Espando Studio Base - fino a 11 client/accessi','DataType':'radioText', 'Tag':'2108113','Style':'hidden','Index':10}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108123 - Espando Studio Base - fino a 12 client/accessi','DataType':'radioText', 'Tag':'2108123','Style':'hidden','Index':11}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108133 - Espando Studio Base - fino a 13 client/accessi','DataType':'radioText', 'Tag':'2108133','Style':'hidden','Index':12}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108143 - Espando Studio Base - fino a 14 client/accessi','DataType':'radioText', 'Tag':'2108143','Style':'hidden','Index':13}"),
            //    new InputItem("{'Key':'postazioniEspando','Text':'2108153 - Espando Studio Base - fino a 15 client/accessi','DataType':'radioText', 'Tag':'2108153','Style':'hidden','Index':14}"),
            //}));

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                new InputItem("{'Key':'postazioniEspando','Text':'Espando Studio Base client/accessi','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xBase'}"),
                new InputItem("{'Key':'postazioniEspando','Text':'Espando Studio Base  accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xBase'}"),
                //new InputItem("{'Key':'postazioniEspando','Text':'2008058 - Laser Espando Studio Base 50 Soggetti','DataType':'integer','MinValue':1,'MaxValue':1,'DefaultValue':1,'Tag':'2008058'}"),
                //new InputItem("{'Key':'postazioniEspando','Text':'9308519 - 1 gestione privacy con 5 incaricati + Check Security 1 interrogazione','DataType':'integer','MinValue':1,'MaxValue':1,'DefaultValue':1,'Tag':'9308519'}"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggDsk");
            b1.Condition.IfOutputOfActivityContainsItem("lic", "Standard");

            Branch b2 = a.CreateBranchTo("uploadFile");
            b2.Condition.IfOutputOfActivityContainsItem("lic", "Demo");
        }

        private void _AddActivity_ESLightDsk(Workflow wf)
        {
            Activity a = wf.CreateActivity("eslDsk");
            a.Title = "Quale tipo di configurazione desideri attivare?";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Configurazione da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("2138003","Espando Studio Light Workstation","2138003"),
                new InputItem("2148003","Espando Studio Light - Workstation + TuttOK (Telematici Entratel)","2148003"),
                new InputItem("2138023","Espando Studio Light fino a 2 client/accessi","2138023"),
                new InputItem("2148023","Espando Studio Light - fino a 2 client/accessi + TuttOK (Telematici Entratel)","2148023"),
                new InputItem("2138033","Espando Studio Light Workstation fino a 3 client/accessi","2138033"),
                new InputItem("2148033","Espando Studio Light fino a 3 client/accessi + TuttOK (Telematici Entratel)","2148033")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggDsk");
            b1.Condition.IfOutputOfActivityContainsItem("lic", "Standard");

            Branch b2 = a.CreateBranchTo("uploadFile");
            b2.Condition.IfOutputOfActivityContainsItem("lic", "Demo");
        }

        private void _AddActivity_ESModulareDsk(Workflow wf)
        {
            Activity a = wf.CreateActivity("esmDsk");
            a.Title = "Quale tipo di configurazione desideri attivare?";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Configurazione da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("2228006","2228006 - ESPANDO STUDIO MODULARE - Modulo Contabile","2228006")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modClientAccessiAggDskModulare");
            b1.Condition.IfOutputOfActivityContainsItem("lic", "Standard");

            Branch b2 = a.CreateBranchTo("uploadFile");
            b2.Condition.IfOutputOfActivityContainsItem("lic", "Demo");
        }

        //private void _AddActivity_ESEAggiuntiva(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("eseAgg");
        //    a.Title = "Desideri attivare anche questo articolo?";  //A.Lucchi(27/04/2021)
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //         new InputItem("2208144", "2208144 - Espando Studio Estesa - Oltre 15 ogni client/accesso aggiuntivo","2208144"),
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("modAggDsk");
        //}

        //private void _AddActivity_TipoContrattoCloud(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("tipoContrattoCloud");
        //    a.Title = "Che tipo di contratto vuoi attivare?";
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //       new InputItem("nContr", "Nuovo contratto"),
        //       new InputItem("upgrade", "Upgrade"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("esCloud");
        //    b1.Condition.IfOutputContainsItem("nContr");
        //}

        //private void _AddActivity_ESCloud(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("esCloud");
        //    a.Title = "Che articoli desideri attivare?";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //          new InputItem("2228008","ESPANDO STUDIO MODULARE CLOUD","2228008"),
        //         new InputItem("2208008","Espando studio cloud Workstation","2208008")  ,
        //         new InputItem("2208028","Espando studio cloud fino a 2 accesso utente","2208028")  ,
        //         new InputItem("2208038","Espando studio cloud fino a 3 accesso utente","2208038")  ,
        //         new InputItem("2208048","Espando studio cloud fino a 4 accesso utente","2208048")  ,
        //         new InputItem("2208058","Espando studio cloud fino a 5 accesso utente","2208058")  ,
        //         new InputItem("2208068","Espando studio cloud fino a 6 accesso utente","2208068")  ,
        //         new InputItem("2208078","Espando studio cloud fino a 7 accesso utente","2208078")  ,
        //         new InputItem("2208088","Espando studio cloud fino a 8 accesso utente","2208088")  ,
        //         new InputItem("2208098","Espando studio cloud fino a 9 accesso utente","2208098")  ,
        //         new InputItem("2208108","Espando studio cloud fino a 10 accesso utente","2208108")  ,
        //         new InputItem("2208118","Espando studio cloud fino a 11 accesso utente","2208118")  ,
        //         new InputItem("2208128","Espando studio cloud fino a 12 accesso utente","2208128")  ,
        //         new InputItem("2208138","Espando studio cloud fino a 13 accesso utente","2208138")  ,
        //         new InputItem("2208148","Espando studio cloud fino a 14 accesso utente","2208148")  ,
        //         new InputItem("2208158","Espando studio cloud fino a 15 accesso utente","2208158")  ,
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("escloudAgg");
        //}

        private void _AddActivity_ModuliAggiuntiviDsk(Workflow wf)
        {
            Activity a = wf.CreateActivity("modAggDsk");
            a.Title = "Desideri inserire dei moduli aggiuntivi?";
            a.TestoRiepilogo = "Moduli aggiuntivi da inserire:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                 new InputItem("2948001","2948001 - Redditometro ","2948001")  ,
                 new InputItem("2958001","2958001 - Import prime note da Home Banking","2958001" )  ,
                 //new InputItem("2008041","2008041 - Licenza mobile","2008041" )  ,
                 new InputItem("2958241","2958241 - Integrazione anagrafiche con Cerved Fino a 1.000 interrogazioni","2958241")  ,
                 new InputItem("2958245","2958245 - Integrazione anagrafiche con Cerved Upgrade per ogni ulteriori 500 interrogazioni","2958245 ")  ,
                 new InputItem("6108123","6108123 - Dati catasto con Cerved fino a 10 interrogazioni","6108123" )  ,
                 new InputItem("6208123","6208123 - Dati catasto con Cerved fino a 20 interrogazioni","6208123" )  ,
                 new InputItem("6508123","6508123 - Dati catasto con Cerved fino a 50 interrogazioni","6508123" )  ,
                 new InputItem("2108003","2108003.MOB - Licenza Mobile Espando Studio Base","2108003.MOB" )  ,
                 new InputItem("2208003","2208003.MOB - Licenza Mobile Espando Studio Estesa","2208003.MOB" )  ,
                 new InputItem("2138003","2138003.MOB - Licenza Mobile Espando Studio Light","2138003.MOB" )  ,
                 new InputItem("2148003","2148003.MOB - Licenza Mobile Espando Studio Light + TuttOk (Telematici Entratel)","2148003.MOB" )  ,
                 new InputItem("2228006","2228006.MOB - Licenza Mobile Espando Studio Modulare","2228006.MOB" )  ,
                 
                 //new InputItem("2978001","2978001 - Firma digitale Dike 6 PRO","2978001" ),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("modClientAccessiAggDsk");
            b1.Condition.IfOutputOfActivityContainsItem("tipoAttivazione", "upgrade");

            Branch b2 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuliAggiuntiviDskUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("modAggDskUpgrade");
            a.Title = "Desideri inserire dei moduli aggiuntivi? <span style='font-size:20px'>1 di 2</span>";
            a.TestoRiepilogo = "Moduli aggiuntivi da inserire:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                 new InputItem("2948001","2948001 - Redditometro ","2948001")  ,
                 new InputItem("2958001","2958001 - Import prime note da Home Banking","2958001" )  ,
                 //new InputItem("2008041","2008041 - Licenza mobile","2008041" )  ,
                 new InputItem("2958241","2958241 - Integrazione anagrafiche con Cerved Fino a 1.000 interrogazioni","2958241")  ,
                 new InputItem("2958245","2958245 - Integrazione anagrafiche con Cerved Upgrade per ogni ulteriori 500 interrogazioni","2958245 ")  ,
                 new InputItem("6108123","6108123 - Dati catasto con Cerved fino a 10 interrogazioni","6108123" )  ,
                 new InputItem("6208123","6208123 - Dati catasto con Cerved fino a 20 interrogazioni","6208123" )  ,
                 new InputItem("6508123","6508123 - Dati catasto con Cerved fino a 50 interrogazioni","6508123" )  ,
                 new InputItem("2108003","2108003.MOB - Licenza Mobile Espando Studio Base","2108003.MOB" )  ,
                 new InputItem("2208003","2208003.MOB - Licenza Mobile Espando Studio Estesa","2208003.MOB" )  ,
                 new InputItem("2138003","2138003.MOB - Licenza Mobile Espando Studio Light","2138003.MOB" )  ,
                 new InputItem("2148003","2148003.MOB - Licenza Mobile Espando Studio Light + TuttOk (Telematici Entratel)","2148003.MOB" )  ,
                 new InputItem("2228006","2228006.MOB - Licenza Mobile Espando Studio Modulare","2228006.MOB" )  ,
                 
                 //new InputItem("2978001","2978001 - Firma digitale Dike 6 PRO","2978001" ),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("modClientAccessiAggDsk");
            b1.Condition.IfOutputOfActivityContainsItem("tipoAttivazione", "upgrade");

            Branch b2 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuliClientAccessiAggiuntiviDskModulare(Workflow wf)
        {
            Activity a = wf.CreateActivity("modClientAccessiAggDskModulare");
            a.Title = "Desideri inserire dei clienti/accessi aggiuntivi?";
            a.TestoRiepilogo = "Moduli aggiuntivi da inserire:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'postazioniESDsk','Text':'2008146 - Client/accesso aggiuntivo Espando Studio Modulare','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'2008146'}"),
        }));

            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuliClientAccessiAggiuntiviDsk(Workflow wf)
        {
            Activity a = wf.CreateActivity("modClientAccessiAggDsk");
            a.Title = "Desideri inserire dei clienti/accessi aggiuntivi? <span style='font-size:20px'>2 di 2</span>";
            a.TestoRiepilogo = "Moduli aggiuntivi da inserire:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                 new InputItem("{'Key':'postazioniESDsk','Text':'2008146 - Client/accesso aggiuntivo Espando Studio Modulare','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'2008146'}"),
                 new InputItem("{'Key':'postazioniESDsk','Text':'2208145 - Espando Studio Base - client/accesso aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'2208145'}"),
                 new InputItem("{'Key':'postazioniESDsk','Text':'2208144 - Espando Studio Estesa - client/accesso aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'2208144'}"),
        }));

            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;
            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        //private void _AddActivity_ESCloudAggiuntiva(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("escloudAgg");
        //    a.Title = "Desideri attivare anche questi articoli?";  //A.Lucchi(27/04/2021)
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //          new InputItem("6008059","Espando telematici","6008059")
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("modAggCloud");
        //}

        //private void _AddActivity_ModuliAggiuntiviCloud(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("modAggCloud");
        //    a.Title = "Che moduli aggiuntivi desideri attivare?";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("2948001","Redditometro ","2948001" )  ,
        //        new InputItem("2958001","Import prime note da Home Banking","2958001" )  ,
        //        new InputItem("EX.ANA.01","Integrazione con Cerved fino a 1000 interrogazioni","EX.ANA.01")  ,
        //        new InputItem("6108123","Dati catasto con Cerved fino a 10 interrogazioni","6108123" )  ,
        //        new InputItem("6208123","Dati catasto con Cerved fino a 20 interrogazioni","6208123" )  ,
        //        new InputItem("6508123","Dati catasto con Cerved fino a 50 interrogazioni","6508123" )  ,
        //        new InputItem("2978001","Firma digitale Dike 6 PRO","2978001" ),
        //        new InputItem("2978009","Firma Remota Namirial","2978009" )
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

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
