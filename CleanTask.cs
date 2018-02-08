using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class CleanTask
    {      
        private int numberOfCommand;
        public int NumberOfCommand
        {
            get
            {
                return this.numberOfCommand;
            }
            set
            {
               if(value > Globales.MAXCOMMANDS)
                {
                    this.numberOfCommand = Globales.MAXCOMMANDS;
                }
                else
                {
                    this.numberOfCommand = value;
                }
            }
        }
        public Vertex StartLocation { get; set; }


        private ICollection<Move> moves;
        public ICollection<Move> Moves
        {
            get
            {
                return this.moves;
            }

            set
            {
                this.moves = value;
            }

        }


        public void AddMove(Move move)
        {
            if (this.moves == null)
                this.moves = new List<Move>();
            if (this.moves.Count() < this.numberOfCommand)
            {
                this.moves.Add(move);
                if (this.moves.Count() == this.numberOfCommand)
                {
                    OnStartClean(EventArgs.Empty);
                }
            }
        }

        public event EventHandler StartClean;
        protected virtual void OnStartClean(EventArgs e)
        {
            EventHandler handler = StartClean;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
