using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowEspandoCloud : Workflow
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

        public WorkflowEspandoCloud(string key, string title, Action<StateContext> drawPage/*, bool possiedeContratto*/) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> activities = GetActivities(typeof(WorkflowEspandoCloud));

            //this.possiedeContratto = possiedeContratto;

            foreach (string a in activities)
            {
                MethodInfo m = this.GetType().GetMethod(a, BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(this, new object[] { this });
            }
        }

        // private void _AddActivity_TipoLicenza(Workflow wf)
        // {
        //     Activity a = wf.CreateActivity("lic");
        //     a.Title = "Che tipo di licenza desideri attivare?";
        //     a.TestoRiepilogo = "Tipo di licenza da attivare:";
        //     //a.Description = "Breve descrizione...";
        //     //a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
        //     //    //new InputItem("demo", "Demo"),
        //     //    //new InputItem("stan", "Standard"),
        //     //     new InputItem("{'Key':'tipoLicenza','Text':'Demo','DataType':'radioText', 'Tag':'tipoLicenza','Style':'visible','Index':0}"),
        //     //      new InputItem("{'Key':'tipoLicenza','Text':'Standard','DataType':'radioText', 'Tag':'tipoLicenza', 'Style':'hidden','Index':1}"),
        //     //}));
        //     a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
        //{
        //         new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(7).ToShortDateString()),
        //         new InputItem("standard","Standard"),
        //         new InputItem("migrazione","Migrazione")
        //}));
        //     a.DrawPage = _DrawPage;

        //     //Branch b1 = a.CreateBranchTo("ver");

        //     Branch b1 = a.CreateBranchTo("migrazioneESC");
        //     b1.Condition.IfOutputContainsItem("migrazione");

        //     Branch b2 = null;
        //     //if(!possiedeContratto)
        //     //    b1 = a.CreateBranchTo("esCloud");
        //     //else
        //     b2 = a.CreateBranchTo("tipoAtt");
        // }

        //private void _AddActivity_MigrazioneESC(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("migrazioneESC");
        //    a.Title = "Attendere a completare questa sezione";
        //    a.StaticInput = null;
        //    a.AllowNoChoice = true;
        //    a.DrawPage = _DrawPage;
        //    Branch b1 = a.CreateBranchToOutcome();
        //}


        private void _AddActivity_TipoAttivazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoAtt");
            a.Title = "Che tipo di attivazione desideri effettuare?";
            a.TestoRiepilogo = "Tipo di attivazione:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("nAtt", "Nuova attivazione"),
                new InputItem("migrazioneESC", "Migrazione"),
                new InputItem("upgrade", "Upgrade"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("TipoESC");
            //Branch b1 = a.CreateBranchTo("tipoContrattoDsk");
            b1.Condition.IfOutputContainsItem("nAtt");

            Branch b2 = a.CreateBranchTo("migrESC");
            //Branch b1 = a.CreateBranchTo("tipoContrattoDsk");
            b2.Condition.IfOutputContainsItem("migrazioneESC");

            Branch b3 = a.CreateBranchTo("modAggCloud");
            //Branch b2 = a.CreateBranchTo("tipoContrattoCloud");
            b3.Condition.IfOutputContainsItem("upgrade");
        }

        private void _AddActivity_TipoESC(Workflow wf)
        {
            Activity a = wf.CreateActivity("TipoESC");
            //a.Title = "Quale modulo desideri attivare?<br><span style='font-size:14px'>La seguente procedura, una volta completata, crea automaticamente lo studio</span>";
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("ESC", "Espando Studio Cloud","2978002.SAAS;SIG.DIKE;$Esploro-Tile"),
                new InputItem("ESCLight", "Espando Studio Cloud light","2978002.SAAS;SIG.DIKE"),
                new InputItem("ESCModulare", "Espando Studio Modulare  Cloud","$Esploro-Tile"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("esCloud");
            b1.Condition.IfOutputContainsItem("ESC");

            Branch b2 = a.CreateBranchTo("esCloudLight");
            b2.Condition.IfOutputContainsItem("ESCLight");

            Branch b3 = a.CreateBranchTo("escModulare");
            b3.Condition.IfOutputContainsItem("ESCModulare");
        }

        private void _AddActivity_TipoESCMigrazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("migrESC");
            //a.Title = "Quale modulo desideri attivare?<br><span style='font-size:14px'>La seguente procedura, una volta completata, crea automaticamente lo studio</span>";
            a.Title = "Quale modulo desideri attivare?";
            a.TestoRiepilogo = "Modulo da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("ESC", "Espando Studio Cloud","2978002.SAAS;SIG.DIKE;$Esploro-Tile"),
                new InputItem("ESCLight", "Espando Studio Cloud light","2978002.SAAS;SIG.DIKE"),
                new InputItem("ESCModulare", "Espando Studio Modulare  Cloud","$Esploro-Tile"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("esCloudMigr");
            b1.Condition.IfOutputContainsItem("ESC");

            Branch b2 = a.CreateBranchTo("esCloudLightMigr");
            b2.Condition.IfOutputContainsItem("ESCLight");

            Branch b3 = a.CreateBranchTo("escModulareMigr");
            b3.Condition.IfOutputContainsItem("ESCModulare");
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

        //private void _AddActivity_TipoESDesktop(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("esDsk");
        //    a.Title = "Che tipo contratto desideri attivare?";  //A.Lucchi(27/04/2021)
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //       new InputItem("ese", "Espando Studio Estesa"),
        //       new InputItem("esb", "Espando Studio Base"),
        //       new InputItem("esl", "Espando Studio Light"),
        //       new InputItem("esm", "Espando Studio Modulare")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("eseDsk");
        //    b1.Condition.IfOutputContainsItem("ese");

        //    Branch b2 = a.CreateBranchTo("esbDsk");
        //    b2.Condition.IfOutputContainsItem("esb");

        //    Branch b3 = a.CreateBranchTo("eslDsk");
        //    b3.Condition.IfOutputContainsItem("esl");

        //    Branch b4 = a.CreateBranchTo("esmDsk");
        //    b4.Condition.IfOutputContainsItem("esm");
        //}

        //private void _AddActivity_ESEstesaDsk(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("eseDsk");
        //    a.Title = "Che articoli desideri attivare?";  //A.Lucchi(27/04/2021)
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //         new InputItem("2208003","Espando Studio Estesa - Workstation","2208003"),
        //         new InputItem("2208023","Espando Studio Estesa - fino a 2 client/accessi","2208023"),
        //         new InputItem("2208033","Espando Studio Estesa - fino a 3 client/accessi","2208033"),
        //         new InputItem("2208043","Espando Studio Estesa - fino a 4 client/accessi","2208043"),
        //         new InputItem("2208053","Espando Studio Estesa - fino a 5 client/accessi","2208053"),
        //         new InputItem("2208063","Espando Studio Estesa - fino a 6 client/accessi","2208063"),
        //         new InputItem("2208073","Espando Studio Estesa - fino a 7 client/accessi","2208073"),
        //         new InputItem("2208083","Espando Studio Estesa - fino a 8 client/accessi","2208083"),
        //         new InputItem("2208093","Espando Studio Estesa - fino a 9 client/accessi","2208093"),
        //         new InputItem("2208103","Espando Studio Estesa - fino a 10 client/accessi","2208103"),
        //         new InputItem("2208113","Espando Studio Estesa - fino a 11 client/accessi","2208113"),
        //         new InputItem("2208123","Espando Studio Estesa - fino a 12 client/accessi","2208123"),
        //         new InputItem("2208133","Espando Studio Estesa - fino a 13 client/accessi","2208133"),
        //         new InputItem("2208143","Espando Studio Estesa - fino a 14 client/accessi","2208143"),
        //         new InputItem("2208153","Espando Studio Estesa - fino a 15 client/accessi","2208153"),
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("eseAgg");
        //}

        //private void _AddActivity_ESBaseDsk(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("esbDsk");
        //    a.Title = "Che articoli desideri attivare?";  //A.Lucchi(27/04/2021)
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("2108003","Espando Studio Base - Workstation","2108003"),
        //        new InputItem("2108023","Espando Studio Base - fino a 2 client/accessi","2108023"),
        //        new InputItem("2108033","Espando Studio Base - fino a 3 client/accessi","2108033"),
        //        new InputItem("2108043","Espando Studio Base - fino a 4 client/accessi","2108043"),
        //        new InputItem("2108053","Espando Studio Base - fino a 5 client/accessi","2108053"),
        //        new InputItem("2108063","Espando Studio Base - fino a 6 client/accessi","2108063"),
        //        new InputItem("2108073","Espando Studio Base - fino a 7 client/accessi","2108073"),
        //        new InputItem("2108083","Espando Studio Base - fino a 8 client/accessi","2108083"),
        //        new InputItem("2108093","Espando Studio Base - fino a 9 client/accessi", "2108093"),
        //        new InputItem("2108103","Espando Studio Base - fino a 10 client/accessi","2108103"),
        //        new InputItem("2108113","Espando Studio Base - fino a 11 client/accessi","2108113"),
        //        new InputItem("2108123","Espando Studio Base - fino a 12 client/accessi","2108123"),
        //        new InputItem("2108133","Espando Studio Base - fino a 13 client/accessi","2108133"),
        //        new InputItem("2108143","Espando Studio Base - fino a 14 client/accessi","2108143"),
        //        new InputItem("2108153","Espando Studio Base - fino a 15 client/accessi","2108153"),
        //        new InputItem("2008058","Laser Espando Studio Base 50 Soggetti ","2008058")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("modAggDsk");
        //}

        //private void _AddActivity_ESLightDsk(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("eslDsk");
        //    a.Title = "Che articoli desideri attivare?";  //A.Lucchi(27/04/2021)
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("2138003","Espando Studio Light Workstation","2138003"),
        //        new InputItem("2148003","Espando Studio Light - Workstation + TuttOK (Telematici Entratel)","2148003"),
        //        new InputItem("2138023","Espando Studio Light fino a 2 client/accessi","2138023"),
        //        new InputItem("2148023","Espando Studio Light - fino a 2 client/accessi + TuttOK (Telematici Entratel)","2148023"),
        //        new InputItem("2138033","Espando Studio Light Workstation fino a 3 client/accessi","2138033"),
        //        new InputItem("2148033","Espando Studio Light fino a 3 client/accessi + TuttOK (Telematici Entratel)","2148033")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("modAggDsk");
        //}

        //private void _AddActivity_ESModulareDsk(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("esmDsk");
        //    a.Title = "Che articoli desideri attivare?";  //A.Lucchi(27/04/2021)
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //        new InputItem("2228006","ESPANDO STUDIO MODULARE - Modulo Contabile","2228006")
        //    }));
        //    a.DrawPage = _DrawPage;

        //    Branch b1 = a.CreateBranchTo("modAggDsk");
        //}

        //private void _AddActivity_ESEAggiuntiva(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("eseAgg");
        //    a.Title = "Desideri attivare anche questo articolo?";  //A.Lucchi(27/04/2021)
        //    //a.Description = "Breve descrizione...";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //         new InputItem("2208144", "Espando Studio Estesa - Oltre 15 ogni client/accesso aggiuntivo","2208144"),
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

        private void _AddActivity_ESCloud(Workflow wf)
        {
            Activity a = wf.CreateActivity("esCloud");
            a.Title = "Quale tipo di configurazione desideri attivare?";
            a.TestoRiepilogo = "Configurazione da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                //new InputItem("{'Key':'postazioniESC','Text':'Espando studio cloud accessi utente','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xCloud'}"),
                //new InputItem("{'Key':'postazioniESC','Text':'Espando studio cloud  accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xCloud'}"),
                new InputItem("{'Key':'postazioniESC','Text':'Espando studio cloud accessi utente','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xCloud'}"),
                new InputItem("{'Key':'postazioniESC','Text':'Espando Studio Cloud accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xCloud'}"),

            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggCloud");
        }

        private void _AddActivity_ESCloudLight(Workflow wf)
        {
            Activity a = wf.CreateActivity("esCloudLight");
            a.Title = "Quale tipo di configurazione desideri attivare?";
            a.TestoRiepilogo = "Configurazione da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("2138009", "2138009 - Workstation", "2138009"),
               new InputItem("2148009", "2148009 - Workstation + Espando telematici (Telematici Entratel)", "2148009;60000530;6518009"),
               new InputItem("2138029", "2138029 - fino a 2 client/accessi", "2138029"),
               new InputItem("2148029", "2148029 - fino a 2 client/accessi + Espando telematici (Telematici Entratel)", "2148029;60000530;6518009"),
               new InputItem("2130039", "2130039 - fino a 3 client/accessi", "2130039"),
               new InputItem("2148039", "2148039 - fino a 3 client/accessi + Espando telematici (Telematici Entratel)", "2148039;60000530;6518009"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggCloud");
        }

        private void _AddActivity_ESCloudModulare(Workflow wf)
        {
            Activity a = wf.CreateActivity("escModulare");
            a.Title = "Quale tipo di configurazione desideri attivare?";
            a.TestoRiepilogo = "Configurazione da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
               new InputItem("{'Key':'postazioniESC','Text':'Espando studio modulare cloud accessi utente','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xCloudMod'}")
                //new InputItem("{'Key':'postazioniESC','Text':'Espando studio modulare cloud accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xCloudMod'}"),

            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggCloud");
        }

        private void _AddActivity_ESCloudMigr(Workflow wf)
        {
            Activity a = wf.CreateActivity("esCloudMigr");
            a.Title = "Quale tipo di configurazione desideri attivare?";
            a.TestoRiepilogo = "Configurazione da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                //new InputItem("{'Key':'postazioniESC','Text':'Espando studio cloud accessi utente','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xCloud'}"),
                //new InputItem("{'Key':'postazioniESC','Text':'Espando studio cloud  accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xCloud'}"),
                new InputItem("{'Key':'postazioniESC','Text':'Espando studio cloud accessi utente','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xCloud'}"),
                new InputItem("{'Key':'postazioniESC','Text':'Espando Studio Cloud accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xCloud'}"),

            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggCloudMigr");
        }

        private void _AddActivity_ESCloudLightMigr(Workflow wf)
        {
            Activity a = wf.CreateActivity("esCloudLightMigr");
            a.Title = "Quale tipo di configurazione desideri attivare?";
            a.TestoRiepilogo = "Configurazione da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("2138009", "2138009 - Workstation", "2138009"),
               new InputItem("2148009", "2148009 - Workstation + Espando Telematici (Telematici Entratel)", "2148009;60000530;6518009"),
               new InputItem("2138029", "2138029 - fino a 2 client/accessi", "2138029"),
               new InputItem("2148029", "2148029 - fino a 2 client/accessi + Espando Telematici (Telematici Entratel)", "2148029;60000530;6518009"),
               new InputItem("2130039", "2130039 - fino a 3 client/accessi", "2130039"),
               new InputItem("2148039", "2148039 - fino a 3 client/accessi + Espando Telematici (Telematici Entratel)", "2148039;60000530;6518009"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggCloudMigr");
        }

        private void _AddActivity_ESCloudModulareMigr(Workflow wf)
        {
            Activity a = wf.CreateActivity("escModulareMigr");
            a.Title = "Quale tipo di configurazione desideri attivare?";
            a.TestoRiepilogo = "Configurazione da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
               new InputItem("{'Key':'postazioniESC','Text':'Espando studio modulare cloud accessi utente','DataType':'integer','MinValue':1,'MaxValue':15,'DefaultValue':1,'Tag':'xCloudMod'}")
                //new InputItem("{'Key':'postazioniESC','Text':'Espando studio modulare cloud accesso aggiuntivo (oltre 15 accessi)','DataType':'integer','MinValue':1,'MaxValue':100,'DefaultValue':1,'Tag':'xCloudMod'}"),

            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("modAggCloudMigr");
        }

        //private void _AddActivity_ModuliAggiuntiviDsk(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("modAggDsk");
        //    a.Title = "Che moduli aggiuntivi desideri attivare?";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //         new InputItem("2948001","Redditometro ","2948001")  ,
        //         new InputItem("2958001","Import prime note da Home Banking","2958001" )  ,
        //         new InputItem("2008041","Licenza mobile","2008041" )  ,
        //         new InputItem("EX.ANA.01","Integrazione con Cerved fino a 1000 interrogazioni","EX.ANA.01")  ,
        //         new InputItem("6108123","Dati catasto con Cerved fino a 10 interrogazioni","6108123" )  ,
        //         new InputItem("6208123","Dati catasto con Cerved fino a 20 interrogazioni","6208123" )  ,
        //         new InputItem("6508123","Dati catasto con Cerved fino a 50 interrogazioni","6508123" )  ,
        //         new InputItem("2978001","Firma digitale Dike 6 PRO","2978001" )
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;
        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        //private void _AddActivity_ESCloudAggiuntiva(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("escloudAgg");
        //    a.Title = "Desideri attivare anche questi articoli?";  //A.Lucchi(27/04/2021)
        //    a.TestoRiepilogo = "Articoli da attivare:";
        //    a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
        //          new InputItem("6008059","6008059 - Espando telematici","6008059")
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("modAggCloud");
        //}

        //private void _AddActivity_ModuliAggiuntiviCloud(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("modAggCloud");
        //    a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>1 di 2</span>";
        //    a.TestoRiepilogo = "Moduli aggiuntivi da attivare:";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //         //new InputItem("60000530","60000530 - Espando telematici (500 anagrafiche) ","60000530"),
        //        new InputItem("2958241.SAAS","2958241.SAAS - Integrazione con Cerved","2958241.SAAS")  ,
        //        new InputItem("6108123.SAAS","6108123.SAAS - Dati catasto con Cerved fino a 10 interrogazioni","6108123.SAAS" )  ,
        //         new InputItem("6208123.SAAS","6208123.SAAS - Dati catasto con Cerved fino a 20 interrogazioni","6208123.SAAS" )  ,
        //         new InputItem("6508123.SAAS","6508123.SAAS - Dati catasto con Cerved fino a 50 interrogazioni","6508123.SAAS" )  ,
        //        new InputItem("2978009","2978009 - Firma Remota Namirial","2978009" ),
        //        //new InputItem("2978001","2978001 - Firma digitale Dike 6 PRO","2978001" ),
        //        //new InputItem("7818003.SAAS","7818003.SAAS - Analisi di Bilancio","7818003.SAAS" ),
        //        new InputItem("7828003.SAAS","7828003.SAAS - Bilancio Consolidato","7828003.SAAS" ),
        //        new InputItem("7308013.SAAS","7308013.SAAS - 730 Professionisti","7308013.SAAS"),
        //        new InputItem("7328009","7328009 - 730 CAF generico","7328009")
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("modAggCloudExtra");
        //}

        private void _AddActivity_ModuliAggiuntiviCloud(Workflow wf)
        {
            Activity a = wf.CreateActivity("modAggCloud");
            a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>1 di 3</span>";
            a.TestoRiepilogo = "Moduli aggiuntivi da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                //new InputItem("2958249","2958249 - Integrazione anagrafiche con Cerved", "2958241.SAAS"),
                //new InputItem("6108129","6108129 - Dati catasto con Cerved - fino a 10 interrogazioni","6108123.SAAS"),
                //new InputItem("6208129","6208129 - Dati catasto con Cerved - fino a 20 interrogazioni","6208123.SAAS" ),
                //new InputItem("6508129","6508129 - Dati catasto con Cerved - fino a 50 interrogazioni","6508123.SAAS" ),
                //new InputItem("2978009","2978009 - Firma remota Namirial", "2978009" ),
                //new InputItem("GSTUDIO.SAAS","GSTUDIO.SAAS - StudiOk Gestione Studio - 1 accesso","GSTUDIO.SAAS"),
                //new InputItem("4008016.SAAS","4008016.SAAS - StudiOk Parcellazione Fatturazione", "4008016.SAAS"),
                //new InputItem("4008056.SAAS","4008056.SAAS - StudiOk Preventivi e Contratti","4008056.SAAS")

                new InputItem("2958241.SAAS","2958241.SAAS - Integrazione anagrafiche con Cerved  (1000 interrogazioni)","2958241.SAAS"),
                new InputItem("2958245.SAAS","2958245.SAAS - Integrazione anagrafiche con Cerved (500 interrogazioni)  ","2958245.SAAS"),
                new InputItem("6108123.SAAS","6108123.SAAS - Dati catasto con Cerved - fino a 10 interrogazioni","6108123.SAAS"),
                new InputItem("6208123.SAAS","6208123.SAAS - Dati catasto con Cerved - fino a 20 interrogazioni","6208123.SAAS"),
                new InputItem("6508123.SAAS","6508123.SAAS - Dati catasto con Cerved - fino a 50 interrogazioni","6508123.SAAS"),
                new InputItem("2978009","2978009 - Firma remota Namirial","2978009")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("modAggCloudExtra");
        }

        //private void _AddActivity_ModuliAggiuntiviCloudExtra(Workflow wf)
        //{
        //    Activity a = wf.CreateActivity("modAggCloudExtra");
        //    a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>2 di 2</span>";
        //    a.TestoRiepilogo = "Moduli aggiuntivi da attivare:";
        //    a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
        //        new InputItem("GSTUDIO","GSTUDIO.SAAS - StudiOK Gestione Studio - 1 client/accesso","GSTUDIO.SAAS" ),
        //        new InputItem("4008016","4008016.SAAS - Studiok Parcellazione/Fatturazione","4008016.SAAS" ),
        //        new InputItem("4008056","4008056.SAAS - StudiOk - Preventivi e Contratti","4008056.SAAS" ),
        //    }));
        //    a.DrawPage = _DrawPage;
        //    a.AllowNoChoice = true;

        //    Branch b1 = a.CreateBranchTo("uploadFile");
        //}

        private void _AddActivity_ModuliAggiuntiviCloudExtra(Workflow wf)
        {
            Activity a = wf.CreateActivity("modAggCloudExtra");
            a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>2 di 3</span>";
            a.TestoRiepilogo = "Moduli aggiuntivi da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                //new InputItem("7818003","7818003 - Analisi di Bilancio" ,"7818003"),
                //new InputItem("7828003","7828003 - Bilancio Consolidato","7828003") ,
                //new InputItem("7208003.SAAS","7208003.SAAS - Certificazione Unica","7208003.SAAS") ,
                //new InputItem("7308013.SAAS","7308013.SAAS - 730 Professionisti","7308013.SAAS") ,
                //new InputItem("7309003.SAAS","7309003.SAAS - 730 Caf generico","7309003.SAAS") ,
                //new InputItem("7328009","7328009 - 730 Caf DOC","7328009")

                new InputItem("GSTUDIO.SAAS","GSTUDIO.SAAS - StudiOk Gestione Studio - 1 accesso","GSTUDIO.SAAS"),
                new InputItem("4008016.SAAS","4008016.SAAS - StudiOk Parcellazione Fatturazione","4008016.SAAS"),
                new InputItem("4008056.SAAS","4008056.SAAS - StudiOk Preventivi e Contratti","4008056.SAAS"),
                new InputItem("7828003.SAAS","7828003.SAAS Bilancio Consolidato","7828003.SAAS"),
                new InputItem("7208003.SAAS","7208003.SAAS - Certificazione Unica","7208003.SAAS"),
                new InputItem("7308013.SAAS","7308013.SAAS - 730 Professionisti","7308013.SAAS"),
                new InputItem("7309003.SAAS","7309003.SAAS - 730 Caf generico","7309003.SAAS")

            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("modAggCloudExtraPag3");
        }


        private void _AddActivity_ModuliAggiuntiviCloudExtraPag3(Workflow wf)
        {
            Activity a = wf.CreateActivity("modAggCloudExtraPag3");
            a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>3 di 3</span>";
            a.TestoRiepilogo = "Moduli aggiuntivi da attivare:";
            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                //new InputItem("{'Key':'postazioniESCPag3Upgrade','Text':'Espando studio cloud accessi utente','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'xCloudUpgrade'}"),
                new InputItem("{'Key':'postazioniESCPag3Upgrade','Text':'Espando Studio Cloud - client/accesso aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':10,'DefaultValue':1,'Tag':'xCloudUpgrade'}"),
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }


        private void _AddActivity_ModuliAggiuntiviCloudMigr(Workflow wf)
        {
            Activity a = wf.CreateActivity("modAggCloudMigr");
            //a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>1 di 2</span>";
            a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>1 di 3</span>";
            a.TestoRiepilogo = "Moduli aggiuntivi da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
            //    new InputItem("2958241.SAAS","2958241.SAAS - Integrazione con Cerved","2958241.SAAS")  ,
            //    new InputItem("6108123.SAAS","6108123.SAAS - Dati catasto con Cerved fino a 10 interrogazioni","6108123.SAAS" )  ,
            //     new InputItem("6208123.SAAS","6208123.SAAS - Dati catasto con Cerved fino a 20 interrogazioni","6208123.SAAS" )  ,
            //     new InputItem("6508123.SAAS","6508123.SAAS - Dati catasto con Cerved fino a 50 interrogazioni","6508123.SAAS" )  ,
            //    new InputItem("2978009","2978009 - Firma Remota Namirial","2978009" ),
            //    //new InputItem("2978001","2978001 - Firma digitale Dike 6 PRO","2978001" ),
            //    //new InputItem("7818003.SAAS","7818003.SAAS - Analisi di Bilancio","7818003.SAAS" ),
            //    new InputItem("7828003.SAAS","7828003.SAAS - Bilancio Consolidato","7828003.SAAS" ),
            //    new InputItem("7308013.SAAS","7308013.SAAS - 730 Professionisti","7308013.SAAS"),
            //    new InputItem("7328009","7328009 - 730 CAF generico","7328009")

            new InputItem("2958241.SAAS", "2958241.SAAS - Integrazione anagrafiche con Cerved  (1000 interrogazioni)", "2958241.SAAS"),
                new InputItem("2958245.SAAS", "2958245.SAAS - Integrazione anagrafiche con Cerved (500 interrogazioni)  ", "2958245.SAAS"),
                new InputItem("6108123.SAAS", "6108123.SAAS - Dati catasto con Cerved - fino a 10 interrogazioni", "6108123.SAAS"),
                new InputItem("6208123.SAAS", "6208123.SAAS - Dati catasto con Cerved - fino a 20 interrogazioni", "6208123.SAAS"),
                new InputItem("6508123.SAAS", "6508123.SAAS - Dati catasto con Cerved - fino a 50 interrogazioni", "6508123.SAAS"),
                new InputItem("2978009", "2978009 - Firma remota Namirial", "2978009")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("modAggCloudExtraMigr");
        }

        private void _AddActivity_ModuliAggiuntiviCloudExtraMigr(Workflow wf)
        {
            Activity a = wf.CreateActivity("modAggCloudExtraMigr");
            //a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>2 di 2</span>";
            a.Title = "Vuoi attivare moduli aggiuntivi? <span style='font-size:20px'>2 di 3</span>";
            a.TestoRiepilogo = "Moduli aggiuntivi da attivare:";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                //new InputItem("GSTUDIO","GSTUDIO.SAAS - StudiOK Gestione Studio - 1 client/accesso","GSTUDIO.SAAS" ),
                //new InputItem("4008016","4008016.SAAS - Studiok Parcellazione/Fatturazione","4008016.SAAS" ),
                //new InputItem("4008056","4008056.SAAS - StudiOk - Preventivi e Contratti","4008056.SAAS" ),

                new InputItem("GSTUDIO.SAAS","GSTUDIO.SAAS - StudiOk Gestione Studio - 1 accesso","GSTUDIO.SAAS"),
                new InputItem("4008016.SAAS","4008016.SAAS - StudiOk Parcellazione Fatturazione","4008016.SAAS"),
                new InputItem("4008056.SAAS","4008056.SAAS - StudiOk Preventivi e Contratti","4008056.SAAS"),
                new InputItem("7828003.SAAS","7828003.SAAS Bilancio Consolidato","7828003.SAAS"),
                new InputItem("7208003.SAAS","7208003.SAAS - Certificazione Unica","7208003.SAAS"),
                new InputItem("7308013.SAAS","7308013.SAAS - 730 Professionisti","7308013.SAAS"),
                new InputItem("7309003.SAAS","7309003.SAAS - 730 Caf generico","7309003.SAAS")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            //Branch b1 = a.CreateBranchTo("uploadFile");
            Branch b1 = a.CreateBranchTo("modAggCloudExtraPag3");
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
