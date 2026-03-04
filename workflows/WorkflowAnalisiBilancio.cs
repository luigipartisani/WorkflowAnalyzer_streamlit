using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
    public class WorkflowAnalisiBilancio : Workflow
    {
        private Action<StateContext> _DrawPage { get; set; }

        private int tipoLicenza { get; set; } //0 -> niente, 1-> comm, 2 -> azi

        private List<string> ShowMethods(Type type)
        {
            List<string> methods = new List<string>();

            foreach (var method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.Name.StartsWith("_AddActivity_")) methods.Add(method.Name);
            }

            return methods;
        }

        public WorkflowAnalisiBilancio(string key, string title, Action<StateContext> drawPage, int tipoLicenza) : base(key, title)
        {
            _DrawPage = drawPage;

            List<string> methods = ShowMethods(typeof(WorkflowAnalisiBilancio));

            this.tipoLicenza = tipoLicenza;

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

            Branch b1 = null;
            if (tipoLicenza == 0)
            {
                b1 = a.CreateBranchTo("tipoCliente");
            }
            else
            {
                b1 = a.CreateBranchTo("ver");
            }
        }

        private void _AddActivity_TipoCliente(Workflow wf)
        {
            Activity a = wf.CreateActivity("tipoCliente");
            a.Title = "Quale tipo di soggetto vuoi abilitare?";
            a.TestoRiepilogo = "Tipo di soggetto:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("prof", "Professionista", "PDS.COMM"),
                new InputItem("azi", "Azienda", "PDS.AZI"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("ver");
        }

        private void _AddActivity_Versione(Workflow wf)
        {
            Activity a = wf.CreateActivity("ver");
            a.Title = "Quale configurazione desideri attivare?";
            a.TestoRiepilogo = "Configurazione da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("meno5pdl", "Fino a 5 posti di lavoro"),
                new InputItem("oltre5pdl", "Oltre 5 posti di lavoro")
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("studioMeno5Pdl");
            b1.Condition.IfOutputContainsItem("meno5pdl");

            Branch b2 = a.CreateBranchTo("studioOltre5Pdl");
            b2.Condition.IfOutputContainsItem("oltre5pdl");
        }

        private void _AddActivity_AttivaAzienda(Workflow wf)
        {
            Activity a = wf.CreateActivity("studioMeno5Pdl");
            a.Title = "Quale versione desideri attivare?";
            a.TestoRiepilogo = "Versione da attivare:";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
                new InputItem("4808053", "4808053 - 5 aziende", "4808053"),
                new InputItem("4808103", "4808103 - 10 Aziende", "4808103"),
                new InputItem("4808003", "4808003 - Aziende Illimitate", "4808003"),
            }));
            a.DrawPage = _DrawPage;

            Branch b1 = a.CreateBranchTo("uploadFile");
        }

        private void _AddActivity_AttivaPdl(Workflow wf)
        {
            Activity a = wf.CreateActivity("studioOltre5Pdl");
            a.Title = "Quale versione desideri attivare?";
            a.TestoRiepilogo = "Versione da attivare:";
            //a.Description = "Breve descrizione...";
            a.StaticInput = new Input(InputType.Single, new List<InputItem>(new InputItem[] {
               new InputItem("4808054", "4808054 - 5 aziende", "4808054 "),
                new InputItem("4808104", "4808104 - 10 Aziende", "4808104 "),
                new InputItem("4808004", "4808004 - Aziende Illimitate", "4808004 "),
            }));
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
