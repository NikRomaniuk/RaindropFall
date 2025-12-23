using Microsoft.Maui.Graphics;
using System.Diagnostics;

namespace RaindropFall
{
    // A specific child member of the Group
    public class GroupMember
    {
        public FlowObject ChildObject { get; set; }
        // offset relative to Group Center (in virtual units where 100 = 100% of GameWidth)
        public double OffsetX { get; set; } // Virtual units (e.g., 5.0 = 5% of GameWidth)
        public double OffsetY { get; set; } // Virtual units (e.g., 5.0 = 5% of GameWidth, same scale as X)
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
        
        // Render distance for spawn/despawn
        public double RenderDistance { get; set; }

        // List of all children in this Group
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();

        // Template data for recreating members on spawn
        private readonly List<ObstacleTemplate> _formationTemplate = new List<ObstacleTemplate>();

        // Constructor - FlowGroup is just a container/organizer, not a moving object
        public FlowGroup(double speed, double renderDistance = 20.0)
        {
            Speed = speed;
            RenderDistance = renderDistance;
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
            var newObstacle = new FlowObject(color, size, this.Speed, this.RenderDistance);

            Members.Add(new GroupMember
            {
                ChildObject = newObstacle,
                OffsetX = offsetX,
                OffsetY = offsetY
            });
        }

        /// <summary>
        /// Recreates all members from template, reusing existing objects when possible to reduce GC pressure
        /// </summary>
        private void RecreateMembers()
        {
            // If we already have the right number of members, reuse them instead of recreating
            if (Members.Count == _formationTemplate.Count)
            {
                // Reuse existing members - just reset their properties
                for (int i = 0; i < Members.Count; i++)
                {
                    var template = _formationTemplate[i];
                    var existingObstacle = Members[i].ChildObject;
                    
                    // Update properties without creating new object
                    existingObstacle.Size = template.Size;
                    existingObstacle.Speed = this.Speed;
                    existingObstacle.Visual.Color = template.Color;
                    existingObstacle.IsActive = true;
                    
                    // Update offsets
                    Members[i].OffsetX = template.OffsetX;
                    Members[i].OffsetY = template.OffsetY;
                }
                return;
            }

            // First time or count mismatch - need to recreate
            // Clear existing members (old FlowObjects will be garbage collected)
            Members.Clear();

            // Recreate all members from template
            foreach (var template in _formationTemplate)
            {
                var newObstacle = new FlowObject(template.Color, template.Size, this.Speed, this.RenderDistance);

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
                        // UpdateUI is already called inside FlowObject.Update(), no need to call it again
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
                // Convert offsets from virtual units to proportional coordinates
                // Both X and Y use the same scale (100 units = GameWidth), ensuring visual consistency
                // Y conversion accounts for aspect ratio so visual distances match
                double offsetXProportional = SceneProperties.ProportionalFromVirtualUnits(member.OffsetX);
                double offsetYProportional = SceneProperties.ProportionalFromVirtualUnitsY(member.OffsetY);
                
                // Spawn at startX + offsetX horizontally
                // Y position is set by FlowObject.Spawn (1.2), then adjusted by offsetY
                member.ChildObject.Spawn(startX + offsetXProportional);
                // Adjust Y position by offsetY (1.2 is the base spawn Y)
                member.ChildObject.Y = member.ChildObject.Y + offsetYProportional;
                
                // DEBUG: Log spawn information
                #if DEBUG && !ANDROID
                Debug.WriteLine($"[FLOWOBJECT SPAWN] Size: {member.ChildObject.Size} units, Speed: {member.ChildObject.Speed} units/s, StartY: {member.ChildObject.Y:F4}, StartX: {member.ChildObject.X:F4}, OffsetX: {member.OffsetX:F4}, OffsetY: {member.OffsetY:F4}");
                #endif
                
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