using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageDictionary : MonoBehaviour {

	public string defaultLanguage;
	public static string waitForOther;
	public static string otherIsGone;
	public static string instructions;

	void Start () {
		SelectedLanguage (defaultLanguage);
	}


	public void SelectedLanguage(string language) {

		if (language == "deutsch") {
			waitForOther = "Bitte warte einen Moment auf den anderen Teilnehmer ...";
			otherIsGone = "Woops ... \n Scheint so, als wäre dein Kollege weg! \n Danke für Ihre Teilnahme!";
			instructions = "Nun, erinnere dich.\n Bewegen Sie sich sehr langsam.\n Synchronisiere deine Bewegungen\n Aufeinander folgen";
		}

		if (language == "french") {
			waitForOther = "Veuillez attendre un moment pour l'autre participant ...";
			otherIsGone = "Woops ... \n On dirait que votre collègue est parti! \n merci de votre participation!";
			instructions = "Maintenant, souvenez-vous \n \n 1. Déplacez-vous très lentement \n 2. Synchronisez vos mouvements \n 3. Suivez-vous";
		}

		if (language == "italian") {
			waitForOther = "Per favore aspetta un momento per l'altro partecipante ...";
			otherIsGone = "Woops ... \n Sembra che il tuo collega se ne sia andato! \n grazie per la tua partecipazione!";
			instructions = "Ora, ricorda \n \n 1. Muoviti molto lentamente \n 2. Sincronizza i tuoi movimenti \n 3. Seguiti";
		}

		if (language == "english") {
			waitForOther = "Please wait for a moment for the other participant...";
			otherIsGone = "Woops... \n Seems like your colleague left! \n thank you for your participation!";
			instructions = "Now, remember \n \n 1. Move very slowly \n 2. Synchronize your movements \n 3. Follow each other";
		}
	}

}
