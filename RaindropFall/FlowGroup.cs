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

    public class FlowGroup : FlowObject
    {
        // List of all children in this Group
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();

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
            var newObstacle = new FlowObject(color, size, this.Speed);

            Members.Add(new GroupMember
            {
                ChildObject = newObstacle,
                OffsetX = offsetX,
                OffsetY = offsetY
            });
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