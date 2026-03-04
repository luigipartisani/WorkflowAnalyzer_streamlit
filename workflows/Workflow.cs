using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BN.WebLicenze.Controllers
{
	public enum InputType { None = 0, Single, Multiple, Numerical, Edit };

	public static class ContextMemory
	{
		public static StateContext Context { get; set; }
		//public static string Key { get; set; }
		public static Workflow Workflow { get; set; }
	}

	public class Workflow
	{
		internal const string SUMMARY = "$Summary";
		internal const string OUTCOME = "$Outcome";

		public string Key { get; }
		public string Title { get; }

		public string Description { get; set; }

		public Workflow(string key, string title)
		{
			this.Key = key;
			this.Title = title;
		}

		public readonly Dictionary<string, Activity> Activities = new Dictionary<string, Activity>();
		public readonly Dictionary<string, State> States = new Dictionary<string, State>();

		public Activity CreateActivity(string key)
		{
			Activity activity = new Activity(key);
			Activities.Add(key, activity);

			return activity;
		}

		public Activity CreateSummaryActivity()
		{
			Activity activity = CreateActivity(Workflow.SUMMARY);
			activity.AllowNoChoice = true;

			Branch b1 = activity.CreateBranchToOutcome();

			return activity;
		}

		public Activity CreateOutcomeActivity()
		{
			Activity activity = CreateActivity(Workflow.OUTCOME);
			activity.AllowNoChoice = true;

			return activity;
		}

		public void Execute()
		{
			_AddState(new State(this.Activities.ElementAt(0).Value));
		}

		private void _AddState(State state)
		{
			state.Order = States.Count;
			States.Add(state.Key, state);

			if (States.Count > 1)
			{
				State previousState = States.Where(x => x.Value.Order == state.Order - 1).SingleOrDefault().Value;
				state.PreviousState = previousState;
				previousState.NextState = state;
			}

			StateContext context = new StateContext()
			{
				State = state,
				PreviousButton = (state.Activity.Key == Workflow.OUTCOME ? null : (state.Order == 0 ? null : "Indietro")),
				NextButton = (state.Activity.Key == Workflow.OUTCOME ? "Chiudi" : (state.Activity.Key == Workflow.SUMMARY ? "Esegui" : "Avanti")),
				ComeBack = _ComeBack,
				GoAhead = _GoAhead,
			};

			//context.Choice = new Choice(state.Activity.StaticInput);

			if (state.Activity.Key == Workflow.SUMMARY)
			{
				string summary = "";

				State s = state.PreviousState;
				while (s != null)
				{
					if (s.Output != null && s.Output.Count > 0)
					{
						string subSummary = "";
						foreach (var o in s.Output)
							if (o.Text.ToUpper() == "Numero".ToUpper())
								subSummary += string.Format("<li>{0}</li>", o.Value);
							else
							{
								if (o.Text.ToUpper() == "GESTIONI" || o.Text.ToUpper() == "BUNDLE" || o.Text.ToUpper() == "UPGRADE")
								{
									subSummary += "";
								}
								else if (o.Text == "Codice postazione" || o.Text == "Codice istituto" || o.Text == "Codice CUC")
								{
									subSummary += string.Format("<li>{0}</li>", o.Text + ":&nbsp;" + o.Tag);  //A.Lucchi
									subSummary = subSummary.Replace("<li></li>", "");
								}
								else if (o.Key == "datiSoggettiWhistle")
								{
									switch (o.Tag.ToString())
									{
										case "nomeSoggettiWhistle1":
										case "nomeSoggettiWhistle2":
										case "nomeSoggettiWhistle3":
											subSummary += string.Format("<li>Nome: {0}</li>", o.Text);
											break;
										case "cognomeSoggettiWhistle1":
										case "cognomeSoggettiWhistle2":
										case "cognomeSoggettiWhistle3":
											subSummary += string.Format("<li>Cognome: {0}</li>", o.Text);
											break;
										case "mailSoggettiWhistle1":
										case "mailSoggettiWhistle2":
										case "mailSoggettiWhistle3":
											subSummary += string.Format("<li>Mail: {0}</li>", o.Text);
											break;
									}
									subSummary = subSummary.Replace("<li></li>", "");
								}
								else
								{
									subSummary += string.Format("<li>{0}</li>", o.Text + (o.Value != 0 ? " " + o.Value : ""));  //A.Lucchi
									subSummary = subSummary.Replace("<li></li>", "");
								}
							}

						//if (!string.IsNullOrEmpty(s.Activity.TestoRiepilogo))
						//{
						if (subSummary != "")
							subSummary = string.Format("<b>{0}</b><ul>{1}</ul>", s.Activity.TestoRiepilogo, subSummary);
						//}
						//else
						//{
						//    subSummary = string.Format("<b>{0}</b><ul>{1}</ul>", s.Activity.Title, subSummary);
						//}
						summary = subSummary + summary;
					}

					s = s.PreviousState;
				}

				summary = string.Format("<div style='font-size:15px'>{0}</div>", summary);
				state.Activity.Description = summary;
			}
			else if (state.Activity.Key == Workflow.OUTCOME)
			{

			}

			state.Activity.DrawPage?.Invoke(context);
		}

		//private void _ComeBack(StateContext context)
		//{
		//    State currentState = context.State;

		//    //Sgancio lo stato corrente dal precedente
		//    State previousState = currentState.PreviousState;
		//    previousState.NextState = null;

		//    //Rimuovo lo stato dal dictionary
		//    States.Remove(currentState.Key);
		//}

		private void _ComeBack(StateContext context)
		{
			State currentState = context.State;

			//Sgancio lo stato corrente dal precedente
			State previousState = currentState.PreviousState;
			if (previousState != null)
			{
				previousState.NextState = null;

				//Rimuovo lo stato dal dictionary
				States.Remove(currentState.Key);

				ContextMemory.Context.State = previousState;
			}
		}

		private void _GoAhead(StateContext context)
		{
			if (!context.State.Activity.AllowNoChoice && context.ResponseContext.SelectedItems.Count == 0)
			{
				if (context.State.Activity.Key != "uploadFile")
				{
					context.ResponseContext.FailAction?.Invoke("Nessun elemento selezionato");
					return;
				}
			}

			context.State.Output = context.ResponseContext.SelectedItems;

			foreach (Branch b in context.State.Activity.Branches)
			{
				b.State = context.State;
				if (b.Condition.IsSatisfied())
				{
					context.ResponseContext.SuccessAction?.Invoke(null);

					_AddState(new State(this.Activities[b.NextActivityKey]));
					return;
				}
			}
		}

		public string GetDiagram()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(string.Format(";{0}:", this.Title));

			foreach (var a in Activities.Values)
			{
				foreach (var b in a.Branches)
				{
					if (Activities.ContainsKey(b.NextActivityKey))
						sb.AppendLine(string.Format("{0}->{1}->{2}",
							a.Key,
							b.Condition.Type.ToString(),
							Activities[b.NextActivityKey].Key));
					else
						sb.AppendLine(string.Format("{0}->{1}->{2}",
							a.Key,
							b.Condition.Type.ToString(),
							"?"));
				}
			}

			return sb.ToString();
		}
	}

	public class StateContext
	{
		public State State;
		//public Choice Choice;

		public string PreviousButton;
		public string NextButton;

		public Action<StateContext> GoAhead { get; set; }
		public Action<StateContext> ComeBack { get; set; }

		public ResponseContext ResponseContext = new ResponseContext();

		public bool IsSummary()
		{
			return (State.Activity.Key == Workflow.SUMMARY);
		}

		public bool IsOutcome()
		{
			return (State.Activity.Key == Workflow.OUTCOME);
		}

		public Dictionary<string, InputItem[]> Choices
		{
			get
			{
				Dictionary<string, InputItem[]> choices = new Dictionary<string, InputItem[]>();

				State s = this.State;
				while (s != null)
				{
					if (!s.Activity.Key.StartsWith("$") && s.Output.Count > 0)
						choices.Add(s.Activity.Key, s.Output.ToArray());

					s = s.PreviousState;
				}

				return choices;
			}
		}
	}

	public class ResponseContext
	{
		public StateContext StateContext;

		public List<InputItem> SelectedItems = new List<InputItem>();

		public Action<object> FailAction { get; set; }
		public Action<object> SuccessAction { get; set; }
	}

	public class State
	{
		public State PreviousState;
		public State NextState;

		public string Key
		{
			get { return string.Format("s({0})", Order); }
		}

		internal State(Activity activity)
		{
			this.Activity = activity;
		}

		public int Order { get; internal set; }
		//public string Name { get; }

		public Activity Activity;
		//public Choice Choice;

		//public List<string> Input;
		public List<InputItem> Output;
	}

	//public class Choice
	//{
	//    public InputType Type { get; set; }
	//    public List<InputItem> Items = new List<InputItem>();

	//    public Choice(Input input)
	//    {
	//        if (input != null)
	//        {
	//            this.Type = input.Type;
	//            this.Items = input.Values;
	//        }
	//    }
	//}

	public class InputItem
	{
		public string Key;
		public string Text;
		public decimal Value;
		public object Tag;
		public bool Selected;

		public dynamic JSON;
		public string fileContent;  //A.Lucchi
		public bool edit; //A.Lucchi(19/05/2021)

		public InputItem()
		{
		}

		public InputItem(string json)
		{
			InputItem input = Newtonsoft.Json.JsonConvert.DeserializeObject<InputItem>(json);

			this.Key = input.Key;
			this.Text = input.Text;
			this.Value = input.Value;
			this.Tag = input.Tag;
			this.Selected = input.Selected;

			this.JSON = this.JSON = json;
		}

		public InputItem(string key, string text, bool selected = false, bool edit = false)
		{
			this.Key = key;
			this.Text = text;
			this.Selected = selected;
			this.edit = !edit;
		}

		public InputItem(string key, string text, object tag, bool selected = false, bool edit = false)
		{
			this.Key = key;
			this.Text = text;
			this.Tag = tag;
			this.Selected = selected;
			this.edit = !edit;
		}

		public InputItem(string key, string text, decimal value, bool selected = false, bool edit = false)
		{
			this.Key = key;
			this.Text = text;
			this.Value = value;
			this.Selected = selected;
			this.edit = !edit;
		}

		public InputItem(string key, string text, decimal value, object tag, bool selected = false, bool edit = false)
		{
			this.Key = key;
			this.Text = text;
			this.Value = value;
			this.Tag = tag;
			this.Selected = selected;
			this.edit = !edit;
		}
	}

	public class Input
	{
		public InputType Type { get; set; }
		public List<InputItem> Values;

		public Input(InputType type, List<InputItem> values)
		{
			this.Type = type;
			this.Values = values;
		}
	}

	public class Activity
	{
		public int Order { get; internal set; }
		public string Key { get; }

		public List<Branch> Branches = new List<Branch>();

		public Input StaticInput { get; set; } //Ha la precedenza rispetto a 'DynamicInput'
		public Action<ResponseContext> DynamicInput { get; set; }

		public string Title;
		public string Description;

		public string TestoRiepilogo { get; set; }

		public bool AllowNoChoice = false;

		public Action<StateContext> DrawPage { get; set; }

		public Activity(string key)
		{
			this.Key = key;
		}

		public Branch CreateBranchTo(string nextActivityKey)
		{
			Branch branch = new Branch(nextActivityKey);

			branch.Order = Branches.Count + 1;

			Branches.Add(branch);

			return branch;
		}

		internal Branch CreateBranchToSummary()
		{
			Branch branch = CreateBranchTo(Workflow.SUMMARY);
			return branch;
		}

		internal Branch CreateBranchToOutcome()
		{
			Branch branch = CreateBranchTo(Workflow.OUTCOME);
			return branch;
		}

	}

	public class Branch
	{
		public int Order { get; internal set; }
		public Condition Condition;
		public string NextActivityKey;

		public State State;

		public Branch(string nextActivityKey)
		{
			this.NextActivityKey = nextActivityKey;
			this.Condition = new Condition(this);
			this.Condition.Else();
		}
	}

	public class Condition
	{
		public enum ConditionType { Else, IfOutputContainsItem, IfOutputOfActivityContainsItem };
		public ConditionType Type;

		//Dictionary<string, object> Parameters;

		public string Item;
		public string Activity;

		public Branch Branch;
		public Condition(Branch branch)
		{
			this.Branch = branch;
		}

		public LinkCondition LinkCondition;

		public bool IsSatisfied()
		{
			bool ret = false;

			switch (Type)
			{
				case ConditionType.Else:
					ret = true;
					break;
				case ConditionType.IfOutputContainsItem:
					ret = Output().Contains(this.Item);
					break;
				case ConditionType.IfOutputOfActivityContainsItem:
					ret = OutputOfActivity(this.Activity).Contains(this.Item);
					if (this.LinkCondition.Or.Item != null)
						ret |= this.LinkCondition.And.IsSatisfied();
					else if (this.LinkCondition.And.Item != null)
						ret &= this.LinkCondition.And.IsSatisfied();
					break;
				default:
					ret = false;
					break;
			}

			return ret;
		}

		//public bool IsSatisfied()
		//{
		//    switch (Type)
		//    {
		//        case ConditionType.Else:
		//            return true;
		//        case ConditionType.IfOutputContainsItem:
		//            return Output().Contains(this.Item);
		//        case ConditionType.IfOutputOfActivityContainsItem:
		//            return OutputOfActivity(this.Activity).Contains(this.Item);
		//        default:
		//            return false;
		//    }
		//}

		public List<InputItem> Output()
		{
			return OutputOfActivity(Branch.State.Activity.Key);
		}

		public List<InputItem> OutputOfActivity(string activity)
		{
			State s = Branch.State;

			while (s.Activity.Key != activity)
			{
				s = s.PreviousState;
				if (s == null) return new List<InputItem>();
			}

			return s.Output;
		}

		//public void IfOutputContainsItem(string item)
		//{
		//    Type = ConditionType.IfOutputContainsItem;

		//    Item = item;
		//}

		public LinkCondition IfOutputContainsItem(string item)
		{
			Type = ConditionType.IfOutputContainsItem;

			Item = item;

			return LinkCondition = new LinkCondition(this);
		}

		//public void IfOutputOfActivityContainsItem(string activity, string item)
		//{
		//    Type = ConditionType.IfOutputOfActivityContainsItem;

		//    Activity = activity;
		//    Item = item;
		//}

		public LinkCondition IfOutputOfActivityContainsItem(string activity, string item)
		{
			Type = ConditionType.IfOutputOfActivityContainsItem;

			Activity = activity;
			Item = item;

			return LinkCondition = new LinkCondition(this);
		}

		public void Else()
		{
			Type = ConditionType.Else;
		}

	}

	public class LinkCondition
	{
		public Condition And;
		public Condition Or;

		public LinkCondition(Condition condition)
		{
			this.And = new Condition(condition.Branch);
			this.Or = new Condition(condition.Branch);
		}
	}

	public static class Extension
	{
		public static bool Contains(this List<InputItem> values, string key)
		{
			var v = values.Where(x => x.Key == key).SingleOrDefault();
			return v != null;
		}
	}
}
