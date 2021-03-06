﻿using System.Collections.Generic;
using Estrella.Zone.Handlers;

namespace Estrella.Zone.Game
{
    public delegate void QuestionCallback(ZoneCharacter character, byte answer);

    public sealed class Question
    {
        public Question(string pText, QuestionCallback pFunction, object obj = null)
        {
            Text = pText;
            Function = pFunction;
            Answers = new List<string>();
            Object = obj;
        }

        public string Text { get; private set; }
        public QuestionCallback Function { get; private set; }
        public List<string> Answers { get; private set; }
        public object Object { get; set; }

        public void Add(params string[] text)
        {
            Answers.AddRange(text);
        }

        public void Send(ZoneCharacter character, ushort distance = (ushort) 1000)
        {
            Handler15.SendQuestion(character, this, distance);
        }
    }
}