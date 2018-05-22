using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageTextDictionary : MonoBehaviour {

	public string defaultLanguage;

	public static string selectedLanguage;
	public static string waitForOther;
	public static string otherIsGone;
	public static string instructions;
	public static string finished;

	void Start () {
		TextPerLanguage (defaultLanguage);
		selectedLanguage = defaultLanguage;
	}

	void Update() {
		LanguageChanged(defaultLanguage); //Temporary, while there is no aditional language input.
	}

	public void LanguageChanged(string language) {
		selectedLanguage = language;
	}

	public void TextPerLanguage(string language) {

		if (language == "deutsch") {
			waitForOther = "Bitte warte einen Moment auf den anderen Teilnehmer ...";
			otherIsGone = "Woops ... \n Scheint so, als wäre dein Kollege weg! \n Danke für Ihre Teilnahme!";
			instructions = "Bitte erinnere dich...\n 1. Bewegen Sie sich sehr langsam.\n 2. Synchronisiere deine Bewegungen\n 3. Aufeinander folgen";
			finished = "Wir hoffen, dass Sie Ihre Erfahrung genossen haben. \n Vielen Dank!";
		}

		if (language == "french") {
			waitForOther = "Veuillez attendre un moment pour l'autre participant ...";
			otherIsGone = "Woops ... \n On dirait que votre collègue est parti! \n Merci de votre participation!";
			instructions = "Maintenant, souvenez-vous... \n \n 1. Déplacez-vous très lentement \n 2. Synchronisez vos mouvements \n 3. Suivez-vous";
			finished = "We hope you enjoyed your experience. \n Thank you!";
		}

		if (language == "italian") {
			waitForOther = "Per favore aspetta un momento per l'altro partecipante ...";
			otherIsGone = "Woops ... \n Sembra che il tuo collega se ne sia andato! \n Grazie per la tua partecipazione!";
			instructions = "Per favore, ricorda... \n \n 1. Muoviti molto lentamente \n 2. Sincronizza i tuoi movimenti \n 3. Seguiti";
			finished = "We hope you enjoyed your experience. \n Thank you!";
		}

		if (language == "english") {
			waitForOther = "Please wait for a moment for the other participant...";
			otherIsGone = "Woops... \n Seems like your colleague left! \n Thank you for your participation!";
			instructions = "Please remember: \n \n 1. Move very slowly \n 2. Synchronize your movements \n 3. Follow each other \n \n \n Focus by adjusting the headset vertically";
			finished = "We hope you enjoyed your experience. \n Thank you!";
		}
	}

}
