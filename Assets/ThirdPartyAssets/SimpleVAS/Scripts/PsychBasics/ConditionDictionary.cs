using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPsychBasics {
    public class ConditionDictionary : MonoBehaviour {

        public List<string> conditions = new List<string>();
        public Dropdown conditionDropdown;

        [HideInInspector]
        public static string[] selectedOrder;

        // Use this for initialization
        void Start () {

            conditionDropdown.ClearOptions();

            List<string> _dropDownOptions = new List<string>();

            foreach (List<string> permu in Permutate(conditions, conditions.Count)) {
                string _option = string.Join(" ", permu.ToArray());
                _dropDownOptions.Add(_option);
            }

            conditionDropdown.AddOptions(_dropDownOptions);
        }
	
        private void RotateRight(List<string> sequence, int count) {
            string tmp = sequence[count - 1];
            sequence.RemoveAt(count - 1);
            sequence.Insert(0, tmp);
        }

        //from https://www.codeproject.com/Articles/43767/A-C-List-Permutation-Iterator
        private IEnumerable<IList> Permutate(List<string> sequence, int count) {
            if (count == 1) yield return sequence;
            else {
                for (int i = 0; i < count; i++) {
                    foreach (var perm in Permutate(sequence, count - 1))
                        yield return perm;
                    RotateRight(sequence, count);
                }
            }
        }


        public void SelectOrder() {
            selectedOrder = conditionDropdown.options[conditionDropdown.value].text.Split(' ');
        }
    }
}