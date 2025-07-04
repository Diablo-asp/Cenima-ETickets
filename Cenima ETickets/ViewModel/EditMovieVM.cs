﻿using Cinema_ETickets.Models;

namespace Cinema_ETickets.ViewModel
{
    public class EditMovieVM
    {
        public Movie Movie { get; set; }

        public List<Cenima>? Cenimas { get; set; }
        public List<Category>? Categories { get; set; }

        public List<int> SelectedActorIds { get; set; } = new();
        public List<Actor> AllActors { get; set; }
    }
}
