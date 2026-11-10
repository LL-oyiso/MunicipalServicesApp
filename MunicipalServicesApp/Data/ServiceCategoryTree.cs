using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.DataStructures;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// The municipality's service hierarchy as a general tree:
    /// root, then department, then the Report Issues category, then the specific
    /// sub-type of problem. It backs the category filter on the Service Request Status
    /// page and tells staff which department a request belongs to.
    /// </summary>
    public static class ServiceCategoryTree
    {
        public const int DepartmentDepth = 1;
        public const int CategoryDepth = 2;
        public const int SubCategoryDepth = 3;

        public static GeneralTree<string> Tree { get; } = Build();

        private static GeneralTree<string> Build()
        {
            var tree = new GeneralTree<string>("All services");

            var infrastructure = tree.Root.AddChild("Infrastructure Services");
            AddCategory(infrastructure, "Roads", "Pothole", "Damaged road sign", "Blocked stormwater drain");

            var energy = tree.Root.AddChild("Electricity and Energy");
            AddCategory(energy, "Electricity", "Street lights", "Power outage", "Exposed wiring");
            AddCategory(energy, "Utilities", "Meter fault");

            var water = tree.Root.AddChild("Water and Sanitation");
            AddCategory(water, "Water", "Burst pipe", "Leaking meter", "No water", "Low pressure");
            AddCategory(water, "Sanitation", "Missed refuse collection", "Illegal dumping", "Sewage overflow");

            var community = tree.Root.AddChild("Community Services");
            AddCategory(community, "Other", "Park maintenance", "Graffiti", "General enquiry");

            return tree;
        }

        private static void AddCategory(GeneralTreeNode<string> department, string category, params string[] subCategories)
        {
            var node = department.AddChild(category);
            foreach (var subCategory in subCategories)
            {
                node.AddChild(subCategory);
            }
        }

        private static GeneralTreeNode<string> FindCategoryNode(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return null;
            }

            return Tree.PreOrder().FirstOrDefault(n =>
                n.Depth == CategoryDepth && string.Equals(n.Value, category, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The department responsible for a category, found by walking up the tree.</summary>
        public static string GetDepartment(string category)
        {
            var node = FindCategoryNode(category);
            return node?.Parent?.Value ?? "Community Services";
        }

        /// <summary>The sub-types staff can choose from when triaging a request in this category.</summary>
        public static List<string> GetSubCategories(string category)
        {
            var node = FindCategoryNode(category);
            return node == null ? new List<string>() : node.Children.Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Whether a request with the given category and sub-type falls under a node
        /// picked in the filter: under a department, its category; under a category, the
        /// category itself; under a sub-type, that exact sub-type.
        /// </summary>
        public static bool Matches(GeneralTreeNode<string> filterNode, string category, string subCategory)
        {
            if (filterNode == null || filterNode.Depth == 0)
            {
                return true;
            }

            var path = GeneralTree<string>.PathFromRoot(filterNode);
            var categoryNode = FindCategoryNode(category);
            if (categoryNode == null)
            {
                return false;
            }

            var requestPath = GeneralTree<string>.PathFromRoot(categoryNode);
            if (filterNode.Depth == SubCategoryDepth)
            {
                return path[CategoryDepth] == categoryNode
                    && string.Equals(filterNode.Value, subCategory, StringComparison.OrdinalIgnoreCase);
            }

            return requestPath[filterNode.Depth] == filterNode;
        }
    }
}
