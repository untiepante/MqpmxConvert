using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureCore.Graphic.Azure.Model.Import.MQO
{
    class CharIndexEnumerator : IEnumerable<char>
    {
        public IEnumerator<char> GetEnumerator()
        {
            for (char i = '0'; i <= '9'; i++)
                yield return i;

            for (char i = 'a'; i <= 'z'; i++)
                yield return i;

            for (char i = 'A'; i <= 'Z'; i++)
                yield return i;

            for (char i = '!'; i <= '/'; i++)
                yield return i;

            for (char i = ':'; i <= '@'; i++)
                yield return i;

            for (char i = ']'; i <= '`'; i++)
                yield return i;

            for (char i = '{'; i <= '~'; i++)
                yield return i;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
