using System.Diagnostics;

namespace RaindropFall
{
    /// <summary>
    /// Efficient collision detection system using spatial partitioning
    /// Only checks collisions for objects that are in proximity
    /// </summary>
    public class CollisionSystem
    {
        private readonly List<ICollidable> _collidables = new List<ICollidable>();
        private readonly HashSet<(ICollidable, ICollidable)> _activeCollisions = new HashSet<(ICollidable, ICollidable)>();
        
        // Spatial grid for efficient collision detection
        private const int GRID_SIZE = 8; // 8x8 grid
        private readonly List<ICollidable>[,] _spatialGrid = new List<ICollidable>[GRID_SIZE, GRID_SIZE];

        public CollisionSystem()
        {
            // Initialize spatial grid
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    _spatialGrid[x, y] = new List<ICollidable>();
                }
            }
        }

        /// <summary>
        /// Register an object for collision detection
        /// </summary>
        public void Register(ICollidable collidable)
        {
            if (!_collidables.Contains(collidable))
            {
                _collidables.Add(collidable);
            }
        }

        /// <summary>
        /// Unregister an object from collision detection
        /// </summary>
        public void Unregister(ICollidable collidable)
        {
            _collidables.Remove(collidable);
        }

        /// <summary>
        /// Check collisions only when requested (not every frame)
        /// Uses spatial partitioning to minimize checks
        /// </summary>
        public void CheckCollisions()
        {
            // Clear the spatial grid
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    _spatialGrid[x, y].Clear();
                }
            }

            // Populate spatial grid with active collidables
            foreach (var collidable in _collidables)
            {
                if (!collidable.IsCollidable) continue;

                var bounds = collidable.GetBounds();
                int gridX = Math.Clamp((int)(bounds.CenterX * GRID_SIZE), 0, GRID_SIZE - 1);
                int gridY = Math.Clamp((int)(bounds.CenterY * GRID_SIZE), 0, GRID_SIZE - 1);

                _spatialGrid[gridX, gridY].Add(collidable);
            }

            // Check collisions within each grid cell and adjacent cells
            _activeCollisions.Clear();
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    var cellObjects = _spatialGrid[x, y];
                    if (cellObjects.Count == 0) continue;

                    // Check within same cell
                    CheckCellCollisions(cellObjects);

                    // Check adjacent cells (right and down to avoid duplicate checks)
                    if (x < GRID_SIZE - 1)
                    {
                        CheckBetweenCells(cellObjects, _spatialGrid[x + 1, y]);
                        if (y < GRID_SIZE - 1)
                            CheckBetweenCells(cellObjects, _spatialGrid[x + 1, y + 1]);
                    }
                    if (y < GRID_SIZE - 1)
                    {
                        CheckBetweenCells(cellObjects, _spatialGrid[x, y + 1]);
                        if (x > 0)
                            CheckBetweenCells(cellObjects, _spatialGrid[x - 1, y + 1]);
                    }
                }
            }
        }

        /// <summary>
        /// Check collisions within a single cell
        /// </summary>
        private void CheckCellCollisions(List<ICollidable> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                for (int j = i + 1; j < objects.Count; j++)
                {
                    CheckCollisionPair(objects[i], objects[j]);
                }
            }
        }

        /// <summary>
        /// Check collisions between two cells
        /// </summary>
        private void CheckBetweenCells(List<ICollidable> cell1, List<ICollidable> cell2)
        {
            foreach (var obj1 in cell1)
            {
                foreach (var obj2 in cell2)
                {
                    CheckCollisionPair(obj1, obj2);
                }
            }
        }

        /// <summary>
        /// Check if two specific objects are colliding
        /// </summary>
        private void CheckCollisionPair(ICollidable obj1, ICollidable obj2)
        {
            if (!obj1.IsCollidable || !obj2.IsCollidable) return;
            if (obj1.CollisionLayer == obj2.CollisionLayer) return; // Same layer objects don't collide

            var bounds1 = obj1.GetBounds();
            var bounds2 = obj2.GetBounds();

            // AABB collision detection
            bool colliding = CheckAABB(bounds1, bounds2);

            var pair = (obj1, obj2);
            bool wasColliding = _activeCollisions.Contains(pair);

            if (colliding && !wasColliding)
            {
                // New collision
                _activeCollisions.Add(pair);
                obj1.OnCollisionEnter(obj2);
                obj2.OnCollisionEnter(obj1);
            }
            else if (!colliding && wasColliding)
            {
                // Collision ended
                _activeCollisions.Remove(pair);
                obj1.OnCollisionExit(obj2);
                obj2.OnCollisionExit(obj1);
            }
        }

        /// <summary>
        /// AABB (Axis-Aligned Bounding Box) collision detection
        /// </summary>
        private bool CheckAABB(CollisionBounds bounds1, CollisionBounds bounds2)
        {
            bool overlapX = Math.Abs(bounds1.CenterX - bounds2.CenterX) < (bounds1.HalfWidth + bounds2.HalfWidth);
            bool overlapY = Math.Abs(bounds1.CenterY - bounds2.CenterY) < (bounds1.HalfHeight + bounds2.HalfHeight);

            return overlapX && overlapY;
        }

        /// <summary>
        /// Clear all registered collidables
        /// </summary>
        public void Clear()
        {
            _collidables.Clear();
            _activeCollisions.Clear();
        }
    }

    /// <summary>
    /// Interface for objects that can collide
    /// </summary>
    public interface ICollidable
    {
        bool IsCollidable { get; }
        CollisionLayer CollisionLayer { get; }
        CollisionBounds GetBounds();
        void OnCollisionEnter(ICollidable other);
        void OnCollisionExit(ICollidable other);
    }

    /// <summary>
    /// Collision layers to determine which objects can collide
    /// </summary>
    public enum CollisionLayer
    {
        Player,
        Obstacle,
        Boundary
    }

    /// <summary>
    /// Bounds information for collision detection
    /// </summary>
    public struct CollisionBounds
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double HalfWidth { get; set; }
        public double HalfHeight { get; set; }

        public CollisionBounds(double centerX, double centerY, double halfWidth, double halfHeight)
        {
            CenterX = centerX;
            CenterY = centerY;
            HalfWidth = halfWidth;
            HalfHeight = halfHeight;
        }
    }
}

