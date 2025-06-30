using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NeuroSdk.Actions;
using NeuroSdk.Json;
using NeuroSdk.Websocket;
using Newtonsoft.Json.Linq;

namespace NeuroScope.Actions
{
    public class CharacterDialogueOptionAction : NeuroAction<string>
    {
        public override string Name => "dialogue_option";

        protected override string Description => Utils.getText(_characterDialogueTree);
        private readonly CharacterDialogueTree _characterDialogueTree;
        private readonly List<string> _options;

        public CharacterDialogueOptionAction(CharacterDialogueTree characterDialogueTree, DialogueBoxVer2 dialogueBox)
        {
            _characterDialogueTree = characterDialogueTree;
            _options = dialogueBox._displayedOptions.Select(option => Utils.stripHtml(option._text)).ToList();
        }

        protected override JsonSchema Schema => new()
        {
            Type = JsonSchemaType.Object,
            Required = new List<string> { "response" },
            Properties = new Dictionary<string, JsonSchema>
            {
                ["response"] = QJS.Enum(_options)
            }
        };

        protected override ExecutionResult Validate(ActionJData actionData, out string response)
        {
            string responseText = actionData.Data?["response"]?.Value<string>();
            if (responseText == null || !_options.Contains(responseText))
            {
                response = null;
                return ExecutionResult.Failure("Action failed. Invalid response choice.");
            }

            response = responseText;
            return ExecutionResult.Success();
        }

        protected override UniTask ExecuteAsync(string response)
        {
            _characterDialogueTree.InputDialogueOption(_options.IndexOf(response));
            return UniTask.CompletedTask;
        }
    }
}