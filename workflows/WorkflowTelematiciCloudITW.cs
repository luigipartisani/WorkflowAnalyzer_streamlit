using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowTelematiciCloudITW : Workflow
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

        public WorkflowTelematiciCloudITW(string key, string title, Action<StateContext> drawPage) : base(key, title)  //A.Lucchi(27/04/2021)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowTelematiciCloudITW));

            //this.possiedeContratto = possiedeContratto;

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
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[]
       {
                //new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(7).ToShortDateString()),
                new InputItem("demo", "Demo - fino al " + DateTime.Now.AddDays(15).ToShortDateString()),
                new InputItem("standard","Standard")
       }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("tipoContratto");
        }

        private void _AddActivity_TipoContratto(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoContratto");
            a.Title = "Quale versione desideri attivare?";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("nContr", "Gestioni"),
                new InputItem("bundle", "Bundle"),
                new InputItem("upgrade", "Upgrade"),
                new InputItem("migrazione", "Migrazione")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("cloud.tel");
            b1.Condition.IfOutputContainsItem("nContr");

            Branch b2 = a.CreateBranchTo("cloud.upgrade");
            b2.Condition.IfOutputContainsItem("upgrade");

            Branch b3 = a.CreateBranchTo("cloud.bundle");
            b3.Condition.IfOutputContainsItem("bundle");

            Branch b4 = a.CreateBranchTo("migrazioneET");
            b4.Condition.IfOutputContainsItem("migrazione");
        }

        private void _AddActivity_TelematiciCloud(Workflow wf)
        {
            Activity a = wf.CreateActivity("cloud.tel");
            a.Title = "Vuoi creare un nuovo contratto? <span style='font-size:20px'>1 di 2</span>";  //A.Lucchi(27/04/2021)
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("entra", "6518009 - Telematici Entratel","6518009"),
                new InputItem("cciaa", "6008199 - Telematici CCIAA","6008199"),
                new InputItem("730", "6008209 - Spese per 730 precompilato","6008209"),
                new InputItem("antie", "6548009 - Adempimenti Antielusione","6548009"), //Check, OK solo su sviluppo
                new InputItem("ascrive", "6008339 - Agenzia scrive","6008339"),  //C'è solo SD e ITW -> Da listino, cambiare la visibilità
                new InputItem("avvTelem", "6008189 - Avvisi Telematici", "6008189")
            }));
            a.DrawPage = _DrawPage;
            a.AllowNoChoice = true;

            Branch b1 = a.CreateBranchTo("cloud.modentra");
        }

        private void _AddActivity_TelematiciCloudUpgrade(Workflow wf)
        {
            Activity a = wf.CreateActivity("cloud.upgrade");
            a.Title = "Vuoi effettuare l'upgrade del contratto?";
            a.TestoRiepilogo = "Moduli da attivare:";

            a.StaticInput = new Input(InputType.Edit, new List<InputItem>(new InputItem[] {
                   new InputItem("{'Key':'upgradeET','Text':'6008019 - Modulo base per gestione ulteriore anagrafiche soggetto (ogni 500)','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'6008019'}"),
                   new InputItem("{'Key':'upgradeET','Text':'6008149 - Accesso utente operatore aggiuntivo','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'6008149'}"),
                   new InputItem("{'Key':'upgradeET','Text':'6018239 - Upgrade alla gestione completa Agenzia Scrive - Civis','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'6018239'}"),
                   new InputItem("{'Key':'upgradeET','Text':'6008169 - Attivazione sedi','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'6008169'}"),
				   new InputItem("{'Key':'upgradeET','Text':'7300654 - Upgrade per ulteriori 200 anagrafiche al Bundle Consulenti del Lavoro','DataType':'integer','MinValue':1,'MaxValue':5,'DefaultValue':1, 'Tag':'7300654'}"),
				}));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_TelematiciCloudBundle(Workflow wf)
        {
            Activity a = wf.CreateActivity("cloud.bundle");
            a.Title = "Quali articoli desideri attivare?";
            a.TestoRiepilogo = "Articoli da attivare:";

            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                  new InputItem("7300053", "7300053 - Modulo base (500anagr) + Telematici Entratel","7300053"),
                  new InputItem("7300153", "7300153 - Modulo Base (500anagr) + Telematici Entratel + Cassetto Fiscale","7300153"),
                  new InputItem("7300653", "7300653 - Modulo Base (200anagr) + Telematici Entratel","7300653"),
            }));

            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_ModuliEntratelCloud(Workflow wf)
        {
            Activity a = wf.CreateActivity("cloud.modentra");
            a.Title = "Quali moduli Entratel desideri attivare? <span style='font-size:20px'>2 di 2</span>";
            a.TestoRiepilogo = "Moduli da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("cassf", "6008159 - Cassetto fiscale","6008159"),
                new InputItem("cat", "6009129 - Dati catastali online","6009129"),
                new InputItem("rate", "6008219 - Rateizzazioni e scadenze", "6008219"),
                new InputItem("firma", "6008139 - Firma autografa automatica", "6008139"),
                //new InputItem("equi", "6568009 - Modulo Equitalia","6568009")
				new InputItem("equi", "6568009 - Modulo Agenzia Riscossione AER","6568009"),
            }));
            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_TelematiciCloudMigrazione(Workflow wf)
        {
            Activity a = wf.CreateActivity("migrazioneET");
            a.Title = "Selezionare i prodotti da attivare con la migrazione <span style='font-size:20px'>1 di 2</span>";
            a.TestoRiepilogo = "Prodotti da attivare con la migrazione:";

            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("entra", "6518009 - Telematici Entratel","6518009"),
                new InputItem("cciaa", "6008199 - Telematici CCIAA","6008199"),
                new InputItem("730", "6008209 - Spese per 730 precompilato","6008209"),
                new InputItem("antie", "6548009 - Adempimenti Antielusione","6548009"), //Check, OK solo su sviluppo
                new InputItem("ascrive", "6008339 - Agenzia scrive","6008339"),  //C'è solo SD e ITW -> Da listino, cambiare la visibilità
                new InputItem("avvTelem", "6008189 - Avvisi Telematici", "6008189")
            }));
            a.AllowNoChoice = true;
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("migrazioneETEntratel");
        }

        private void _AddActivity_TelematiciCloudMigrazioneEntra(Workflow wf)
        {
            Activity a = wf.CreateActivity("migrazioneETEntratel");
            a.Title = "Selezionare i prodotti da attivare con la migrazione  <span style='font-size:20px'>2 di 2</span>";
            a.TestoRiepilogo = "Prodotti da attivare con la migrazione:";

            a.StaticInput = new Input(InputType.Multiple, new List<InputItem>(new InputItem[] {
                new InputItem("cassf", "6008159 - Cassetto fiscale","6008159"),
                new InputItem("cat", "6009129 - Dati catastali online","6009129"),
                new InputItem("rate", "6008219 - Rateizzazioni e scadenze", "6008219"),
                new InputItem("firma", "6008139 - Firma autografa automatica", "6008139"),
                //new InputItem("equi", "6568009 - Modulo Equitalia","6568009")
				new InputItem("equi", "6568009 - Modulo Agenzia Riscossione AER","6568009"),
            }));
            a.AllowNoChoice = true;
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
            a.Title = "La procedura di attivazione si è conclusa";
            a.DrawPage = _DrawPage;
        }
    }
}