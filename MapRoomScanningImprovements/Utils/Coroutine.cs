using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapRoomScanningImprovements.Utils
{
    static class Coroutine
    {
        public static IEnumerator waitFor(IEnumerator enumetator, float seconds)
        {
            if (enumetator == null)
            {
                yield break;
            }

            float cyclesWithinFrame = 1;

            Stack<IEnumerator> stack = new Stack<IEnumerator>();
            stack.Push(enumetator);
            while (stack.Count > 0)
            {
                if (UnityEngine.Time.timeScale <= 0)
                {
                    yield return null;
                    continue;
                }

                if (cyclesWithinFrame < 1)
                {
                    yield return null;
                    cyclesWithinFrame += UnityEngine.Time.deltaTime / seconds;
                    Logger.Debug(string.Format("Skip: Ms {0}, Cycles {1}", UnityEngine.Time.deltaTime, cyclesWithinFrame));
                    continue;
                }

                IEnumerator currentEnumetator = stack.Peek();

                if (currentEnumetator.MoveNext())
                {
                    var currentValue = currentEnumetator.Current;
                    if (currentValue is IEnumerator innerEnumerator)
                    {
                        stack.Push(innerEnumerator);
                    }
                    else
                    {
                        if (cyclesWithinFrame < 2)
                        {
                            cyclesWithinFrame--;

                            yield return null;
                            cyclesWithinFrame += UnityEngine.Time.deltaTime / seconds;
                            Logger.Debug(string.Format("Next: Ms {0}, Cycles {1}", UnityEngine.Time.deltaTime, cyclesWithinFrame));
                        }
                        else
                        {
                            cyclesWithinFrame--;
                        }
                    }
                }
                else
                {
                    stack.Pop();
                }
            }
        }
    }
}
