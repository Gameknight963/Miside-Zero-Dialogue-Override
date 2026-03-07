using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Miside_Zero_Dialouge_Override
{
    public static class NodeMapper
    {
        public static List<DialogueNode> GetAllNodes(this DialogueTree tree)
        {
            List<DialogueNode> visited = new List<DialogueNode>();

            foreach (DialogueNode firstNode in tree.startNodes)
            {
                TraverseNode(firstNode, visited);
            }

            return visited;
        }
        public static List<DialogueNode> TraverseNode(DialogueNode node, List<DialogueNode> visited)
        {
            if (node == null || visited.Contains(node))
                return visited;
            visited.Add(node);

            if (node.nextNodes != null)
            {
                foreach (DialogueNode next in node.nextNodes)
                {
                    TraverseNode(next, visited);
                }
            }
            return visited;
        }
    }
}
