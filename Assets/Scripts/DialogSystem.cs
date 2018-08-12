using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniJSON;

public class DialogNode {
	public readonly long id;
	public readonly string npc;
	public readonly DialogNode[] children;
	
	public DialogNode(long id, string npc, DialogNode[] children) {
		this.id = id;
		this.npc = npc;
		this.children = children;
	}
}

public class DialogTextNode : DialogNode {
	public readonly string[] requireItems;
	public readonly string[] addItems;
	public readonly string[] dialogs;
	public DialogTextNode(long id, string npc, DialogNode[] children, 
						  string[] requireItems, string[] addItems, string[] dialogs) : base(id, npc, children) {
		this.requireItems = requireItems;
		this.addItems = addItems;
		this.dialogs = dialogs;
	}

	public static DialogTextNode Parse(Dictionary<string, object> node) {
		long id = (long)node["id"];
		string npcName = (string)node["npc"];

		DialogNode[] children = null;
		if (node.ContainsKey("children") && node["children"] != null) {
			children = new DialogNode[((List<object>)node["children"]).Count];
		}

		string[] requireItems = null;
		if (node.ContainsKey("require_items") && node["require_items"] != null) {
			requireItems = ((List<object>)node["require_items"]).Select(s => (string)s).ToArray();
		}

		string[] addItems = null;
		if (node.ContainsKey("add_items") && node["add_items"] != null) {
			addItems = ((List<object>)node["add_items"]).Select(s => (string)s).ToArray();
		}

		string[] dialogs = ((List<object>)node["dialogs"]).Select(s => (string)s).ToArray();
		
		return new DialogTextNode(id, npcName, children, requireItems, addItems, dialogs);
	}
}

public class DialogOption {
	public readonly string[] requireItems;
	public readonly string text;
	public bool selected;

	public DialogOption(string[] requireItems, string text) {
		this.requireItems = requireItems;
		this.text = text;
	}
}

public class DialogOptionNode : DialogNode {

	public readonly DialogOption[] options;

	public DialogOptionNode(long id, string npc, DialogNode[] children, 
						  DialogOption[] options) : base(id, npc, children) {
		this.options = options;
	}

	public static DialogOptionNode Parse(Dictionary<string, object> node) {
		long id = (long)node["id"];
		string npcName = (string)node["npc"];

		DialogNode[] children = null;
		if (node.ContainsKey("children") && node["children"] != null) {
			children = new DialogNode[((List<object>)node["children"]).Count];
		}

		List<object> optionNodes = (List<object>)node["options"];
		DialogOption[] options = new DialogOption[optionNodes.Count];
		for (int i = 0; i < options.Length; i++) {
			Dictionary<string, object> optinoNode = (Dictionary<string, object>)optionNodes[i]; 

			string[] requireItems = null;
			if (optinoNode.ContainsKey("require_items") && optinoNode["require_items"] != null) {
				requireItems = ((List<object>)optinoNode["require_items"]).Select(s => (string)s).ToArray();
			}

			string text = (string)optinoNode["text"];

			options[i] = new DialogOption(requireItems, text);
		}
		
		return new DialogOptionNode(id, npcName, children, options);
	}

	public DialogNode Select(string option) {
		for (int i = 0; i < options.Length; i++)
		{
			if (options[i].text.Equals(option)) {
				options[i].selected = true;
				return children[i];
			}
		}
		return null;
	}
}

public class DialogSystem {
	private readonly Dictionary<string, DialogNode> dialogTrees;
	public DialogSystem(Dictionary<string, DialogNode> dialogTrees) {
		this.dialogTrees = dialogTrees;
	}

	public DialogNode this[string key] {	
		get { return dialogTrees[key]; }
	}

	public static DialogSystem Create() {
		return new DialogSystem(LoadResource());
	}

	private static Dictionary<string, DialogNode> LoadResource() {
		string json = (Resources.Load("dialogs") as TextAsset).text;
		Dictionary<string, object> jsonObj = Json.Deserialize(json) as Dictionary<string, object>;

		Dictionary<long, DialogNode> nodeLookup = new Dictionary<long, DialogNode>();

		List<object> dialogNodes = (List<object>)jsonObj["data"];
		foreach (Dictionary<string, object> node in dialogNodes)
		{
			DialogNode n = null;
			string type = (string)node["type"];
			if (type.Equals("text")) {
				n = DialogTextNode.Parse(node);
			} else if (type.Equals("option")) {
				n = DialogOptionNode.Parse(node);
			}

			if (n == null) {
				throw new System.Exception("unknown node type " + type);
			}

			nodeLookup[n.id] = n;
		}

		// Construct the tree
		foreach(Dictionary<string, object> node in dialogNodes) {
			long id = (long)node["id"];
			if (node.ContainsKey("children") && node["children"] != null) {
				List<object> childrenIds = ((List<object>)node["children"]);
				DialogNode dNode = nodeLookup[id];
				for(int i = 0; i < childrenIds.Count; i++) {
					dNode.children[i] = nodeLookup[(long)childrenIds[i]];
				}
			}
		}
		
		Dictionary<string, DialogNode> result = new Dictionary<string, DialogNode>();
		foreach (DialogNode node in nodeLookup.Values) {
			if (!result.ContainsKey(node.npc)) {
				result[node.npc] = node;
			} else {
				if (node.id < result[node.npc].id) {
					result[node.npc] = node;
				}
			}
		}

		return result;
	}
}
