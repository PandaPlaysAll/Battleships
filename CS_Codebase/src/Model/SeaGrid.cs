﻿
using System;
using System.Collections.Generic;


namespace Battleships {

    /// <summary>
    /// The SeaGrid is the grid upon which the ships are deployed.
    /// </summary>
    /// <remarks>
    /// The grid is viewable via the ISeaGrid interface as a read only
    /// grid. This can be used in conjuncture with the SeaGridAdapter to
    /// mask the position of the ships.
    /// </remarks>
    public class SeaGrid : ISeaGrid {

        private const int _WIDTH = 10;
        private const int _HEIGHT = 10;

        private Tile[,] _gameTiles;
        private Dictionary<ShipName, Ship> _ships;


        /// <summary>
        /// The sea grid has changed and should be redrawn.
        /// </summary>
        public event EventHandler Changed;


        /////////////////////////////////////////////////////// Todo /////////////////////////////////////////////////////////
        public TileView this[int x, int y] {
            get { return _gameTiles[x, y].View; }
        }

        /// <summary>
        /// The width of the sea grid.
        /// </summary>
        /// <value>The width of the sea grid.</value>
        /// <returns>The width of the sea grid.</returns>
        public int Width { 
            get => _WIDTH;
        }

        /// <summary>
        /// The height of the sea grid
        /// </summary>
        /// <value>The height of the sea grid</value>
        /// <returns>The height of the sea grid</returns>
        public int Height {
            get => _HEIGHT;
        }

        /// <summary>
        /// ShipsKilled returns the number of ships killed
        /// </summary>
        public int ShipsKilled { get; private set; } = 0;

        /// <summary>
        /// AllDeployed checks if all the ships are deployed
        /// </summary>
        public bool AllDeployed {
            get {
                foreach (Ship s in _ships.Values) {
                    if (!s.IsDeployed) {
                        return false;
                    }
                }
                return true;
            }
        }


        /////////////////////////////////////////////////////// Todo /////////////////////////////////////////////////////////
        /// <summary>
        /// Show the tile view
        /// </summary>
        /// <param name="x">x coordinate of the tile</param>
        /// <param name="y">y coordiante of the tile</param>
        /// <returns></returns>
        //public TileView get_Item(int x, int y) {

        //    return _gameTiles[x, y].View;

        //}

        /// <summary>
        /// SeaGrid constructor, a seagrid has a number of tiles stored in an array
        /// </summary>
        public SeaGrid( Dictionary<ShipName, Ship> ships ) {
            _gameTiles = new Tile[Width, Height];
            // fill array with empty Tiles
            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {
                    _gameTiles[i, j] = new Tile(i, j, null);
                }
            }

            _ships = ships;

        }

        /// <summary>
        /// MoveShips allows for ships to be placed on the seagrid
        /// </summary>
        /// <param name="row">the row selected</param>
        /// <param name="col">the column selected</param>
        /// <param name="ship">the ship selected</param>
        /// <param name="direction">the direction the ship is going</param>
        public void MoveShip( int row, int col, ShipName ship, Direction direction ) {

            Ship newShip = _ships[ship];
            newShip.Remove();
            AddShip(row, col, direction, newShip);

        }

        /// <summary>
        /// Adds a ship to the SeaGrid
        /// </summary>
        /// <param name="row">row coordinate</param>
        /// <param name="col">col coordinate</param>
        /// <param name="direction">direction of ship</param>
        /// <param name="newShip">the ship</param>
        private void AddShip (int row, int col, Direction direction, Ship newShip) {

            try {
                int size = newShip.Size;
                int currentRow = row;
                int currentCol = col;
                int dRow, dCol;

                if (direction == Direction.LeftRight) {
                    dRow = 0;
                    dCol = 1;
                }
                else {
                    dRow = 1;
                    dCol = 0;
                }

                // place ship's tiles in array and into ship object
                for (int i = 0; i < size; i++) {
                    if (currentRow < 0 | currentRow >= Width | currentCol < 0 | currentCol >= Height) {
                        throw new InvalidOperationException("Ship can't fit on the board");
                    }
                    _gameTiles[currentRow, currentCol].Ship = newShip;
                    currentCol += dCol;
                    currentRow += dRow;
                }

                newShip.Deployed(direction, row, col);
            }
            catch (Exception e) {
                newShip.Remove(); // if fails remove the ship
                throw new ApplicationException(e.Message);
            }
            finally {
                Changed?.Invoke(this, EventArgs.Empty);
            }

        }

        /// <summary>
        /// HitTile hits a tile at a row/col, and whatever tile has been hit, a
        /// result will be displayed.
        /// </summary>
        /// <param name="row">the row at which is being shot</param>
        /// <param name="col">the cloumn at which is being shot</param>
        /// <returns>An attackresult (hit, miss, sunk, shotalready)</returns>
        public AttackResult HitTile(int row, int col) {

            try {
                // tile is already hit
                if (_gameTiles[row, col].Shot) {
                    return new AttackResult(ResultOfAttack.ShotAlready, "have already attacked [" + col + "," + row + "]!", row, col);
                }

                _gameTiles[row, col].Shoot();

                // there is no ship on the tile
                if (_gameTiles[row, col].Ship is null) {
                    return new AttackResult(ResultOfAttack.Miss, "missed", row, col);
                }

                // all ship's tiles have been destroyed
                if (_gameTiles[row, col].Ship.IsDestroyed) {
                    _gameTiles[row, col].Shot = true;
                    ShipsKilled += 1;
                    return new AttackResult(ResultOfAttack.Destroyed, _gameTiles[row, col].Ship, "destroyed the enemy's", row, col);
                }

                // else hit but not destroyed
                return new AttackResult(ResultOfAttack.Hit, "hit something!", row, col);
            }
            finally {
                Changed?.Invoke(this, EventArgs.Empty);
            }

        }

    }

}
