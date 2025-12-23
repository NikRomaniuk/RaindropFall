using Microsoft.Maui.Graphics;
using System.Diagnostics;

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

    public class FlowGroup
    {
        // Shared speed for all members
        public double Speed { get; set; }

        // List of all children in this Group
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();

        // Template data for recreating members on spawn
        private readonly List<ObstacleTemplate> _formationTemplate = new List<ObstacleTemplate>();

        // Constructor - FlowGroup is just a container/organizer, not a moving object
        public FlowGroup(double speed)
        {
            Speed = speed;
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
        /// Called every frame to update all members
        /// Returns False if all members have despawned
        /// </summary>
        public bool Update(double deltaTime)
        {
            bool anyActive = false;

            // Update each member individually
            for (int i = 0; i < Members.Count; i++)
            {
                if (Members[i].ChildObject.IsActive)
                {
                    bool isStillActive = Members[i].ChildObject.Update(deltaTime);
                    if (isStillActive)
                    {
                        anyActive = true;
                        // UpdateUI is called inside FlowObject.Update, but we ensure it's called here too
                        Members[i].ChildObject.UpdateUI();
                    }
                }
            }

            return anyActive;
        }

        /// <summary>
        /// Spawns all members at positions relative to startX
        /// Each member spawns at startX + offsetX horizontally
        /// </summary>
        public void Spawn(double startX)
        {
            // Recreate all members as new FlowObjects
            RecreateMembers();

            // Spawn each member at its relative position
            foreach (var member in Members)
            {
                // Spawn at startX + offsetX horizontally
                // Y position is set by FlowObject.Spawn (1.2), then adjusted by offsetY
                member.ChildObject.Spawn(startX + member.OffsetX);
                // Adjust Y position by offsetY (1.2 is the base spawn Y)
                member.ChildObject.Y = member.ChildObject.Y + member.OffsetY;
                
                // DEBUG: Log spawn information
                Debug.WriteLine($"[FLOWOBJECT SPAWN] Size: {member.ChildObject.Size}%, Speed: {member.ChildObject.Speed}, StartY: {member.ChildObject.Y:F4}, StartX: {member.ChildObject.X:F4}, OffsetX: {member.OffsetX:F4}, OffsetY: {member.OffsetY:F4}");
                
                // Ensure member is active and visible
                member.ChildObject.IsActive = true;
                member.ChildObject.Visual.IsVisible = true;
                
                // Update UI to reflect the position
                member.ChildObject.UpdateUI();
            }
        }

        /// <summary>
        /// Updates UI for all active members
        /// </summary>
        public void UpdateUI()
        {
            // Update UI for all active Group members
            foreach (var member in Members)
            {
                if (member.ChildObject.IsActive)
                {
                    member.ChildObject.UpdateUI();
                }
            }
        }
    }
}