using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageTextDictionary : MonoBehaviour {

	public string defaultLanguage;

	public static string selectedLanguage;
    public static string idle;
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
            idle = "Passen Sie das Headset mithilfe der Riemen an den Seiten und oben an, bis Sie mit dem Text zufrieden sind und den Text richtig sehen können. \n Wenn Sie fertig sind, können Sie die unten stehende Schaltfläche aufrufen, um die Erfahrung zu starten.";
            waitForOther = "Bitte warten Sie einen Moment auf den anderen Teilnehmer ...";
			otherIsGone = "Woops ... \n Es scheint so, als wäre die andere Person weg! \n Danke für Ihre Teilnahme!";
			instructions = "Bitte erinneren Sie sich...\n 1. Bewegen Sie sich sehr langsam.\n 2. Stimmen Sie Ihre Bewegungen mit dern anderen Person ab\n 3. Folgen Sie einander";
			finished = "Wir hoffen, dass Sie Ihre Erfahrung genossen haben. \n Vielen Dank!";
			//Bitte schauen Sie diesen Punkt an wenn Sie bereit sind.
			//Bitte den Punkt anschauen wenn Sie bereit sind. 
		}

		if (language == "french") {
            idle = "Ajustez le casque en utilisant les sangles latérales et supérieures jusqu'à ce que vous soyez à l'aise et que vous puissiez voir ce texte correctement. \n Lorsque vous êtes prêt, regardez le bouton ci-dessous pour démarrer l'expérience.";
            waitForOther = "Veuillez attendre un moment pour l'autre participant ...";
			otherIsGone = "Woops ... \n On dirait que votre collègue est parti! \n Merci de votre participation!";
			instructions = "Maintenant, souvenez-vous... \n \n 1. Déplacez-vous très lentement \n 2. Synchronisez vos mouvements \n 3. Suivez-vous";
			finished = "We hope you enjoyed your experience. \n Thank you!";
		}

		if (language == "italian") {
            idle = "Regola l'auricolare usando le cinghie sui lati e in alto fino a quando non ti senti a tuo agio e puoi vedere questo testo correttamente. \n Quando sei pronto, guarda il pulsante qui sotto per iniziare l'esperienza.";
            waitForOther = "Per favore aspetta un momento per l'altro partecipante ...";
			otherIsGone = "Woops ... \n Sembra che il tuo collega se ne sia andato! \n Grazie per la tua partecipazione!";
			instructions = "Per favore, ricorda... \n \n 1. Muoviti molto lentamente \n 2. Sincronizza i tuoi movimenti \n 3. Seguiti";
			finished = "We hope you enjoyed your experience. \n Thank you!";
		}

		if (language == "english") {
            idle = "Adjust the headset using the straps on the sides and on top until you are comfortable and can see this text properly. \n When you are ready, look at the button below to start the experience.";
			waitForOther = "Please wait for a moment for the other participant...";
			otherIsGone = "Woops... \n Seems like your colleague left! \n Thank you for your participation!";
			instructions = "Please remember: \n \n 1. Move very slowly \n 2. Synchronize your movements \n 3. Follow each other \n \n \n Focus by adjusting the headset vertically";
			finished = "We hope you enjoyed your experience. \n Thank you!";
		}
	}

}
