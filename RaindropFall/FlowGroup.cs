using Microsoft.Maui.Graphics;

namespace RaindropFall
{
    // A specific child member of the Group
    public class GroupMember
    {
        public FlowObject ChildObject { get; set; }
        // offset relative to Group Center
        public double OffsetX { get; set; } // REMEBER: 0.0 is center ; -0.1 is left
        public double OffsetY { get; set; } // REMEBER: 0.0 is center ; 0.1 is below
    }

    // Template data for recreating obstacles
    public class ObstacleTemplate
    {
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public Color Color { get; set; }
        public double Size { get; set; }
    }

    public class FlowGroup : FlowObject
    {
        // List of all children in this Group
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();

        // Template data for recreating members on spawn
        private readonly List<ObstacleTemplate> _formationTemplate = new List<ObstacleTemplate>();

        // Constructor passes default values to base
        // The Grouop itself is invisible to act as Anchor
        public FlowGroup(double speed) : base(Colors.Transparent, 100, speed)
        {
            // Make the anchor completely invisible
            Visual.IsVisible = false;
        }

        /// <summary>
        /// Adds a new object to the formation at a fixed offset
        /// </summary>
        public void AddObstacle(double offsetX, double offsetY, Color color, double size)
        {
            // Store template data for recreation
            _formationTemplate.Add(new ObstacleTemplate
            {
                OffsetX = offsetX,
                OffsetY = offsetY,
                Color = color,
                Size = size
            });

            // Create initial obstacle
            var newObstacle = new FlowObject(color, size, this.Speed);

            Members.Add(new GroupMember
            {
                ChildObject = newObstacle,
                OffsetX = offsetX,
                OffsetY = offsetY
            });
        }

        /// <summary>
        /// Recreates all members from template, ensuring fresh FlowObjects
        /// </summary>
        private void RecreateMembers()
        {
            // Clear existing members (old FlowObjects will be garbage collected)
            Members.Clear();

            // Recreate all members from template
            foreach (var template in _formationTemplate)
            {
                var newObstacle = new FlowObject(template.Color, template.Size, this.Speed);

                Members.Add(new GroupMember
                {
                    ChildObject = newObstacle,
                    OffsetX = template.OffsetX,
                    OffsetY = template.OffsetY
                });
            }
        }

        /// <summary>
        /// Called every frame to move the object and its children
        /// Returns False if group has despawned
        /// </summary>
        public override bool Update(double deltaTime)
        {
            bool isStillActive = base.Update(deltaTime);

            if (isStillActive)
            {
                UpdateUI();
            }

            return isStillActive;
        }

        /// <summary>
        /// Overrides the base Spawn to recreate all members as new FlowObjects
        /// </summary>
        public override void Spawn(double startX)
        {
            // Recreate all members as new FlowObjects
            RecreateMembers();

            // Spawn the group itself
            base.Spawn(startX);

            // Ensure all new members are active and visible
            foreach (var member in Members)
            {
                member.ChildObject.IsActive = true;
                member.ChildObject.Visual.IsVisible = true;
            }
        }

        /// <summary>
        /// Overrides the base UpdateUI to move ALL children relative to the Group Center (Anchor)
        /// </summary>
        public override void UpdateUI()
        {
            // Update UI for the Group itself (for position)
            base.UpdateUI();

            // Update UI for all Group members
            for (int i = 0; i < Members.Count; i++)
            {
                // Calculate absolute position based on Group Anchor + Offset
                Members[i].ChildObject.X = this.X + Members[i].OffsetX;
                Members[i].ChildObject.Y = this.Y + Members[i].OffsetY;

                // Update UI for child
                Members[i].ChildObject.UpdateUI();
            }
        }
    }
}