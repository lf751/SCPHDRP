using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FullSerializer;


/// <summary>
/// A language list that can be edited by user
/// </summary>
public struct Language
{
    [SerializeField]
    public string name;
    [SerializeField]
    public string code;
    [SerializeField]
    public int unitynumber;

    public Language(string _name, string _code, int _number)
    {
        name = _name;
        code = _code;
        unitynumber = _number;
    }
}

/// <summary>
/// Meta data for in-game subtitles
/// </summary>
/// 
public struct subtitleMeta
{
    [SerializeField]
    public string subtitle;
    [SerializeField]
    public float delay;
    [SerializeField]
    public float duration;
    [SerializeField]
    public string nextSubtitle;
    [SerializeField]
    public string character;
    [SerializeField]
    public bool noFormat;
}

public static class Localization
{
    static string folderPath = Path.Combine(Application.streamingAssetsPath, GlobalValues.localName);
    static string langCode = "EN";




    static Dictionary<string, Dictionary<string, string>> defStrings = new Dictionary<string, Dictionary<string, string>>();
    static Dictionary<string, Dictionary<string, string>> localStrings = new Dictionary<string, Dictionary<string, string>>();

    static Dictionary<string, subtitleMeta> defSub = new Dictionary<string, subtitleMeta>();
    static Dictionary<string, subtitleMeta> localSub = new Dictionary<string, subtitleMeta>();

    //Localization

    static Dictionary<int, Language> def_langs = new Dictionary<int, Language>()
    {
        {10, new Language ("English", "EN", 10) },
        /*{34, new Language ("Español", "ES", 34) },
        {15, new Language ("Deutsch", "DE", 15) },
        {6, new Language ("简体中文", "CH", 6) },
        {40, new Language ("简体中文", "CH", 6) },
        {41, new Language ("简体中文", "CH", 6) },*/
    };

    static Dictionary<int, Language> langs;


    static public void SetLanguage(int lang)
    {
        langCode = "EN";

        if (lang == -1)
            lang = (int)Application.systemLanguage;

        defStrings = new Dictionary<string, Dictionary<string, string>>();
        localStrings = new Dictionary<string, Dictionary<string, string>>();

        defStrings.Add("uiStrings", GetTable("uiStrings"));
        defStrings.Add("itemStrings", GetTable("itemStrings"));
        defStrings.Add("charaStrings", GetTable("charaStrings"));
        defStrings.Add("playStrings", GetTable("playStrings"));
        defStrings.Add("loadStrings", GetTable("loadStrings"));
        defStrings.Add("deathStrings", GetTable("deathStrings"));
        defStrings.Add("tutoStrings", GetTable("tutoStrings"));
        defSub = GetSubtitles();

        if (langs.ContainsKey(lang))
        {
            langCode = langs[lang].code;

            foreach (var table in defStrings)
            {
                localStrings.Add(table.Key, GetTable(table.Key));
            }
        }
        else
        {
            localStrings.Add("uiStrings", new Dictionary<string, string>());
            localStrings.Add("itemStrings", new Dictionary<string, string>());
            localStrings.Add("playStrings", new Dictionary<string, string>());
            localStrings.Add("charaStrings", new Dictionary<string, string>());
            localStrings.Add("loadStrings", new Dictionary<string, string>());
            localStrings.Add("deathStrings", new Dictionary<string, string>());
            localStrings.Add("tutoStrings", new Dictionary<string, string>());
        }

        localSub = GetSubtitles();
    }



    static public string GetString(string table, string id)
    {
        if (localStrings.ContainsKey(table))
        {
            if (localStrings[table].ContainsKey(id))
            {
                return localStrings[table][id];
            }
            else if (defStrings[table].ContainsKey(id))
            {
                return defStrings[table][id];
            }
            else
                return ("Missing Subtitle: " + id);

        }
        else
            return ("Missing Table " + table);
    }

    static public subtitleMeta GetSubtitle(string id)
    {
        subtitleMeta temp = new subtitleMeta();
        temp.subtitle = "MISSING SUBTITLE";
        temp.noFormat = true;
    

        if (localSub.ContainsKey(id))
        {
            temp = localSub[id];
        }
        else if (defSub.ContainsKey(id))
        {
            temp = defSub[id];
        }

        return temp;
    }

    static public void ExportDefault()
    {
        foreach (var table in defStrings)
        {
            Debug.Log("Exportando tabla " + table.Key);
            SaveTable(table.Key, "EN", table.Value);
        }
    }

    static public void BuildSubsDefault()
    {
        Debug.Log("NO FUCK YOU");
        Dictionary<string, subtitleMeta> tempSubs = new Dictionary<string, subtitleMeta>();
        foreach (var tcadena in sceneStrings_EN)
        {
            subtitleMeta sub = new subtitleMeta();
            sub.subtitle = tcadena.Value;
            sub.delay = 0;
            sub.duration = 6;
            sub.nextSubtitle = "";
            sub.character = "CHANGE_THIS";
            sub.noFormat = false;

            tempSubs.Add(tcadena.Key, sub);
        }

        SaveSub("sceneStrings", "EN", tempSubs);

    }

    static void SaveSub(string FileName, string LanguageCode, Dictionary<string, subtitleMeta> table)
    {
        fsSerializer _serializer = new fsSerializer();

        if (!Directory.Exists(Path.Combine(folderPath, LanguageCode)))
            Directory.CreateDirectory(Path.Combine(folderPath, LanguageCode));

        fsData data;
        _serializer.TrySerialize(table, out data).AssertSuccessWithoutWarnings();

        // emit the data via JSON
        using (StreamWriter streamWriter = File.CreateText(Path.Combine(folderPath, LanguageCode, FileName + ".subs")))
        {
            streamWriter.Write(fsJsonPrinter.PrettyJson(data));
        }
    }

    static void SaveTable(string FileName, string LanguageCode, Dictionary<string, string> table)
    {
        fsSerializer _serializer = new fsSerializer();

        if (!Directory.Exists(Path.Combine(folderPath, LanguageCode)))
            Directory.CreateDirectory(Path.Combine(folderPath, LanguageCode));

        fsData data;
        _serializer.TrySerialize(table, out data).AssertSuccessWithoutWarnings();

        // emit the data via JSON
        using (StreamWriter streamWriter = File.CreateText(Path.Combine(folderPath, LanguageCode, FileName + ".subs")))
        {
            streamWriter.Write(fsJsonPrinter.PrettyJson(data));
        }
    }

    public static void AddMissing()
    {
        Debug.Log("Iniciando Proceso");
        foreach (var table in defStrings)
        {
            Dictionary<string, string> currTable = new Dictionary<string, string>();

            if (!File.Exists(Path.Combine(folderPath, langCode, table.Key + ".subs")))
            {
                Debug.Log("Tabla no existe, agregando todos los valores");
                foreach (var value in table.Value)
                {
                    currTable.Add(value.Key, " MISSING SUBTITLE ");
                }
            }
            else
            {
                Debug.Log("Tabla si existe, examinando paso a paso");
                currTable = localStrings[table.Key];
                foreach (var value in table.Value)
                {
                    if (!localStrings[table.Key].ContainsKey(value.Key))
                    {
                        Debug.Log("Subtitulo faltante en " + value.Key);
                        currTable.Add(value.Key, " MISSING SUBTITLE ");
                    }
                }
            }

            SaveTable(table.Key, langCode, currTable);
        }

        Debug.Log("Subs");

        Dictionary<string, subtitleMeta> currSubs = new Dictionary<string, subtitleMeta>();

        if (!File.Exists(Path.Combine(folderPath, langCode, "sceneStrings" + ".subs")))
        {
            Dictionary<string, string> oldSubs = oldStrings[langCode];
            Debug.Log("Tabla no existe, agregando todos los valores");
            foreach (var value in defSub)
            {
                var val = value.Value;
                if (oldSubs.ContainsKey(value.Key))
                {
                    val.subtitle = oldSubs[value.Key];
                }
                else
                    val.subtitle = "MISSING SUBTITLE";
                currSubs.Add(value.Key, val);
            }
        }
        else
        {
            Debug.Log("Tabla si existe, examinando paso a paso");
            currSubs = localSub;
            foreach (var value in defSub)
            {
                if (!localSub.ContainsKey(value.Key))
                {
                    Debug.Log("Subtitulo faltante en " + value.Key);
                    currSubs.Add(value.Key, value.Value);
                }
            }
        }

        SaveSub("sceneStrings", langCode, currSubs);

    }

    static Dictionary<string, string> GetTable(string FileName)
    {
        Debug.Log("Cargando tabla " + FileName + " en la posicion " + Path.Combine(folderPath, langCode, FileName + ".subs"));
        fsSerializer _serializer = new fsSerializer();
        Dictionary<string, string> loadedTable = new Dictionary<string, string>();

        if (File.Exists(Path.Combine(folderPath, langCode, FileName + ".subs")))
        {
            using (StreamReader streamReader = File.OpenText(Path.Combine(folderPath, langCode, FileName + ".subs")))
            {
                string jsonString = streamReader.ReadToEnd();
                fsData data = fsJsonParser.Parse(jsonString);

                _serializer.TryDeserialize(data, ref loadedTable).AssertSuccessWithoutWarnings();
            }
        }

        return loadedTable;
    }

    static Dictionary<string, subtitleMeta> GetSubtitles()
    {
        fsSerializer _serializer = new fsSerializer();
        Dictionary<string, subtitleMeta> loadedTable = new Dictionary<string, subtitleMeta>();

        Debug.Log("Cargando tabla sceneStrings en la posicion " + Path.Combine(folderPath, langCode, "sceneStrings" + ".subs"));

        if (File.Exists(Path.Combine(folderPath, langCode, "sceneStrings" + ".subs")))
        {
            using (StreamReader streamReader = File.OpenText(Path.Combine(folderPath, langCode, "sceneStrings" + ".subs")))
            {
                string jsonString = streamReader.ReadToEnd();
                fsData data = fsJsonParser.Parse(jsonString);

                _serializer.TryDeserialize(data, ref loadedTable).AssertSuccessWithoutWarnings();
            }
        }

        return loadedTable;
    }

    static public void CheckLangs()
    {
        fsSerializer _serializer = new fsSerializer();


        if (!File.Exists(Path.Combine(folderPath, "languages.langs")))
        {
            fsData data;
            _serializer.TrySerialize(def_langs, out data).AssertSuccessWithoutWarnings();

            // emit the data via JSON
            using (StreamWriter streamWriter = File.CreateText(Path.Combine(folderPath, "languages.langs")))
            {
                streamWriter.Write(fsJsonPrinter.PrettyJson(data));
            }
        }

        using (StreamReader streamReader = File.OpenText(Path.Combine(folderPath, "languages.langs")))
        {
            string jsonString = streamReader.ReadToEnd();
            fsData data = fsJsonParser.Parse(jsonString);

            langs = def_langs;
            _serializer.TryDeserialize(data, ref langs).AssertSuccessWithoutWarnings();
        }
    }

    static public Dictionary<int, Language> GetLangs()
    {
        int number = 0;
        Dictionary<int, Language> langList = new Dictionary<int, Language>()
        {
            {number, new Language ("Auto", "auto", 10) },
        };
        Debug.Log("Parsing Languages, detected " + langs.Count);
        foreach (var lang in langs)
        {
            if (!langList.ContainsValue(lang.Value))
            {
                number++;
                Debug.Log("Language " + lang.Value.name + " number" + number);
                langList.Add(number, lang.Value);
            }

        }

        return (langList);
    }







    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /// 

    public static Dictionary<string, Dictionary<string, string>> oldStrings = new Dictionary<string, Dictionary<string, string>>()
    {
        {"ES", new Dictionary<string, string>() {
            {"BeforeDoorOpen", "<b>{0}</b> : Control, aqui el Agent Ulgrin. Solicito apertura de la celda 3-11."},
            {"ExitCell", "<b>{0}</b> : Hey, tenemos trabajo para ti. Sal de la celda"},
            {"ExitCellRefuse1", "<b>{0}</b> : Tienes retraso? Dije que salgas de la celda. Si no lo haces te sacare a la fuerza"},
            {"ExitCellRefuse2", "<b>{0}</b> : Amigo no tengo todo el dia. Intento ser amable al respecto, pero si no sales, te sacare a la fuerza."},
            {"CellGas1", "<b>{0}</b> : Huh, eres el sujeto de pruebas mas tonto que he conocido. Cierren las puertas y abran el gas nocivo."},
            {"CellGas2", "<b>{0}</b> : Huh, Estoy decepcionado de que no pelearas mas. Tenia tantas ganas de golpearte la cara."},
            {"EscortRun", "<b>{0}</b> : ¡Oye, estupido! Camino equivocado, es por aca!"},
            {"EscortRefuse1", "<b>{0}</b> : ¡Rapido, te estan esperando!"},
            {"EscortRefuse2", "<b>{0}</b> : Mira, yo ya odio mi trabajo. No me lo hagas tan dificil"},
            {"EscortPissedOff1", "<b>{0}</b> : No estoy de humor para esto, No tengo problema en poner una bala en tu cabeza si no empiezas a coperar."},
            {"EscortPissedOff2", "<b>{0}</b> : No estoy de humor para esto, No tengo problema en poner una bala en tu cabeza si no empiezas a coperar."},
            {"EscortKill1", "<b>{0}</b> : ¿Sabes que? Como quieras, usaremos a alguien mas"},
            {"EscortKill2", "<b>{0}</b> : Bien, como quieras traeremos a alguien mas."},


            {"Intro_Convo1_1", "<b>{0}</b> : Y, uh, ¿Como va todo?"},
            {"Intro_Convo1_2", "<b>{0}</b> : Eh, ¿M-me hablas a mi?"},
            {"Intro_Convo1_3", "<b>{0}</b> : Pues si, ¿Quien creeias? ¿Este tonto con la cara golpeable? Claro que te hablo a ti."},
            {"Intro_Convo1_4", "<b>{0}</b> : Oh, solo estoy algo sorprendido. Creo que nunca me habias hablado."},
            {"Intro_Convo1_5", "<b>{0}</b> : Claro, es tu primer dia de trabajo."},
            {"Intro_Convo1_6", "<b>{0}</b> : Nos asignaron hace 5 meses."},
            {"Intro_Convo1_7", "<b>{0}</b> : ¿En serio? Vaya. Que extraño."},

            {"Intro_Convo2_1", "<b>{0}</b> : ¿Has visto alguna buena pelicula?"},
            {"Intro_Convo2_2", "<b>{0}</b> : No soy muy de peliuclas, soy mas de libros."},
            {"Intro_Convo2_3", "<b>{0}</b> : ¿Si? ¿Que clase de libros?"},
            {"Intro_Convo2_4", "<b>{0}</b> : Horror, ciencia ficcion, por ahi."},
            {"Intro_Convo2_5", "<b>{0}</b> : Bromeas."},
            {"Intro_Convo2_6", "<b>{0}</b> : ¿Que tiene?"},
            {"Intro_Convo2_7", "<b>{0}</b> : Todo tu trabajo es de horror y ciencia, pero, sin la parte de la ficcion."},
            {"Intro_Convo2_8", "<b>{0}</b> : Bueno, tenia la idea de escribir un libro sobre las cosas qu-"},
            {"Intro_Convo2_9", "<b>{0}</b> : Mira, no te ofendas, pero no me interesa mucho."},

            {"Intro_Convo3_1", "<b>{0}</b> : Vaya, ya hace hambre. ¿Hoy hay pizza en la cafeteria, cierto?"},
            {"Intro_Convo3_2", "<b>{0}</b> : Eh, creo que hoy es ensalada de atun"},
            {"Intro_Convo3_3", "<b>{0}</b> : Oh, maldita sea. Mi dia esta arruinado. La unica razon por la que sigo trabajando aqui es por la pizza. No se que tiene, pero es deliciosa. Por otro lado, la ensalada de atun es una abominacion asquerosa que deberia encerrarse junto a estas bestias."},
            {"Intro_Convo3_4", "<b>{0}</b> : Uh, okay.."},

            {"Intro_Convo4_1", "<b>{0}</b> : Dejame adivinar, ¿No tienes novia?"},
            {"Intro_Convo4_2", "<b>{0}</b> : Uh, a-are you talking to me?"},
            {"Intro_Convo4_3", "<b>{0}</b> : Course I'm talking to you."},
            {"Intro_Convo4_4", "<b>{0}</b> : ¿..Tan obvio es?"},
            {"Intro_Convo4_5", "<b>{0}</b> : Bueno, no leo mentes, ya que si no estaria encerrado aqui, asi que, si, es bastante obvio"},
            {"Intro_Convo4_6", "<b>{0}</b> : ¿Bueno, como puedo tener novia con un trabajo asi? No puedo hablarle sobre mi dia ni nada"},
            {"Intro_Convo4_7", "<b>{0}</b> : Pues mientele. Dile que trabajas en una cafeteria o algo asi"},
            {"Intro_Convo4_8", "<b>{0}</b> : Bueno, pero... ¿Que tal si olvido lavar mis manos aqui, y al volver a casa las tenga repletas de sangre? ¿Que le digo entonces?"},
            {"Intro_Convo4_9", "<b>{0}</b> : ¡Yo no se!  Dile que es, que es... Ketchup"},
            {"Intro_Convo4_10", "<b>{0}</b> : ¿Ketchup? ¿Porque estaria usando Ketchup en una cafeteria?"},
            {"Intro_Convo4_11", "<b>{0}</b> : Solo olvidalo"},

            {"Intro_Convo5_1", "<b>{0}</b> : ¿Has visto alguna buena pelicula?"},
            {"Intro_Convo5_2", "<b>{0}</b> : No soy mucho de peliculas."},
            {"Intro_Convo5_3", "<b>{0}</b> : Oh, okey. ¿Que tal videojuegos? Eso me hace pensar, alguien deberia hacer un videojuego sobre los monstruos que tenemos aqui."},
            {"Intro_Convo5_4", "<b>{0}</b> : ¿Por que?"},
            {"Intro_Convo5_5", "<b>{0}</b> : No lo se, me parece una idea interesante"},
            {"Intro_Convo5_6", "<b>{0}</b> : ...Bueno, tampoco juego videojuegos"},



            {"Escort1", "<b>{0}</b> : Solo sigueme, oh, y, tenemos autorizacion para matar cualquier sujeto de pruebas, asi que no intentes nada estupido."},
            {"Escort2", "<b>{0}</b> : Solo sigueme, oh, y, tenemos autorizacion para matar cualquier sujeto de pruebas, asi que no intentes nada estupido."},
            {"EscortDone1", "<b>{0}</b> : Bueno, aqui estamos. Entra y haz caso a lo que te digan, y seguramente estaras bien"},
            {"EscortDone2", "<b>{0}</b> : Bien, aqui es. La verdad es que tenia muchas ganas de golpearte, pero bueno."},
            {"EscortDone3", "<b>{0}</b> : Bien, aqui es. La verdad es que me moria de ganas de golpearte, pero, ya que"},
            {"EscortDone4", "<b>{0}</b> : Bueno, aqui estamos. Entra y haz caso a lo que te digan, y puede que estes bien, O puede que no. Me da lo mismo"},
            {"EscortDone5", "<b>{0}</b> : Como sea, no perdamos mas tiempo. Te esperan en la camara"},


            {"EnterChamber","<b>{0}</b> : Atencion Personal Clase-D, entren a la camara de contencion" },
            {"Approach173","<b>{0}</b> : Acerquense al <color=yellow>SCP-173</color> para las pruebas" },
            { "Problem","<b>{0}</b> : Uh, parece haber un problema con el control de la puerta, no responden a nuestros intentos de cerrarla, asi que, Uhm, No rompan el contacto visual con el SCP-173 y esperen ma-" },

            { "Escape1","<b>{0}</b> : Creo que la salida mas rapida, es por el ala sur-este, ¡Sigueme!" },
            { "Escape2","<b>{0}</b> : ¿Escuchaste eso? Ojala no sea -" },






            {"scene_BreachStart_1", "<b>{0}</b> : ¡Agente, detras de ti!"},
            {"scene_BreachStart_2", "<b>{0}</b> : ¡Mierda!"},
            {"scene_BreachStart_3", "<b>{0}</b> : ¡Manten la mirada!"},
            {"scene_BreachStart_4", "<b>{0}</b> : ¡Voy a parpadear, no lo pierdas de vista"},
            {"scene_BreachStart_5", "<b>{0}</b> : ¡Entendido!"},

            {"scene_012_1", "<i>Debo... Terminarlo...</i>"},
            {"scene_012_2", "<i>¿Quieres hacerlo..? Yo no... creo... Que pueda...</i>"},
            {"scene_012_3", "<i>Yo... Yo... debo... hacerlo.</i>"},
            {"scene_012_4", "<i>¡Y-Yo... no... tengo... o-opcion!</i>"},
            {"scene_012_5", "<i>Globos... Esto....Esto no...tiene sentido!</i>"},
            {"scene_012_6", "<i>No... esto... esto es... impossible!</i>"},
            {"scene_012_7", "<i>Nadie puede... ¡Nadie puede completarla!</i>"},
        }
    },
        {"DE", new Dictionary<string, string>() {
            {"BeforeDoorOpen", "<b>{0}</b> : Kontrolle, hier spricht Agent Ulgrin. Ich ersuche das Öffnen der Zelle 3-11."},
            {"ExitCell", "<b>{0}</b> : Hey, die haben ein bisschen Arbeit für dich. Tu mir 'nen Gefallen und tret aus deiner Zelle."},
            {"ExitCellRefuse1", "<b>{0}</b> : Bist du irgendwie dumm oder so? Ich habe gesagt tret aus deiner Zelle. Wenn du nichts aus deiner Zelle tritts kriegst du auf's Maul."},
            {"ExitCellRefuse2", "<b>{0}</b> : Komm schon Kumpel, wir haben nicht den ganzen Tag. Ich versuche hier höflich zu sein. Wenn du nichts aus deiner Zelle tritts kriegst du auf's Maul."},
            {"CellGas1", "<b>{0}</b> : Huh, du musst echt das dümmste Testsubjekt sein das wir jemals hatten. Auch egal, schließt die Tür und öffnet das Gasventil."},
            {"CellGas2", "<b>{0}</b> : Huh, ich bin irgendwie enttäuscht, dass du keinen Stress gemacht hast. Ich hatte gehofft ich könnte dir ins Gesicht schlagen."},
            {"EscortRun", "<b>{0}</b> : Hey, Spast! Das ist der falsche Weg. Komm augenblicklich hier her!"},
            {"EscortRefuse1", "<b>{0}</b> : Beeil dich! Die warten auf dich."},
            {"EscortRefuse2", "<b>{0}</b> : Komm schon Kumpel, ich hasse meinen Job jetzt schon. Wieso machst du ihn mir noch schwerer?"},
            {"EscortPissedOff1", "<b>{0}</b> : Ich hab keinen Bock auf diesen Scheiß, ich habe absolut kein Problem damit dir 'ne Kugel in den Kopf zu jagen wenn du nicht anfängst mitzuarbeiten."},
            {"EscortPissedOff2", "<b>{0}</b> : Ich hab keinen Bock auf diesen Scheiß, ich habe absolut kein Problem damit dir 'ne Kugel in den Kopf zu jagen wenn du nicht anfängst mitzuarbeiten."},
            {"EscortKill1", "<b>{0}</b> : Okay, weißt du was? Dann sei so. Wir werden uns einfach jemand anderen holen."},
            {"EscortKill2", "<b>{0}</b> : Okay, weißt du was? Dann sei so. Wir werden uns einfach jemand anderen holen."},


            {"Intro_Convo1_1", "<b>{0}</b> : Also, uh, wie geht's?"},
            {"Intro_Convo1_2", "<b>{0}</b> : Uh, r-redest du mit mir?"},
            {"Intro_Convo1_3", "<b>{0}</b> : Nunja, mit wem denkst du rede ich, dem D-Klasse Fuzzi hier? Klar red ich mit dir."},
            {"Intro_Convo1_4", "<b>{0}</b> : Oh, ich bin nur ein wenig überrascht. Ich denke, dass ist das erste Mal, das du mit mir redest."},
            {"Intro_Convo1_5", "<b>{0}</b> : Naja, es ist dein erster Arbeitstag hier."},
            {"Intro_Convo1_6", "<b>{0}</b> : Uh, eigentlich arbeiten wir seit fünf Monaten zusammen."},
            {"Intro_Convo1_7", "<b>{0}</b> : Wirklich? Wow. Das ist seltsam."},

            {"Intro_Convo2_1", "<b>{0}</b> : Uh, hast du in letzter Zeit irgendwelche guten Filme gesehen?"},
            {"Intro_Convo2_2", "<b>{0}</b> : Uh, ich schaue nicht wirklich viele Filme, ich lese Bücher."},
            {"Intro_Convo2_3", "<b>{0}</b> : Ja? Was für Bücher?"},
            {"Intro_Convo2_4", "<b>{0}</b> : Uh, Horror, Science-Fiction, sowas halt."},
            {"Intro_Convo2_5", "<b>{0}</b> : Du machst Witze."},
            {"Intro_Convo2_6", "<b>{0}</b> : Wie?"},
            {"Intro_Convo2_7", "<b>{0}</b> : Dein ganzer Job dreht sich um Horror und Science-Fiction, außer, dass es tatsächlich keine Fiktion ist."},
            {"Intro_Convo2_8", "<b>{0}</b> : Also, eigentlich plane ich ein Buch über meine Erfahr-"},
            {"Intro_Convo2_9", "<b>{0}</b> : Ok, nimm's mir nicht übel, aber ich hab jetzt schon jegliches Interesse an dem verloren was du sagen wolltest."},

            {"Intro_Convo3_1", "<b>{0}</b> : Mann, bin ich hungrig. Hey, heute ist Pizzatag in der Cafeteria, stimmt's?"},
            {"Intro_Convo3_2", "<b>{0}</b> : Uh, e-eigentlich ist es Thunfischauflauf, denke ich."},
            {"Intro_Convo3_3", "<b>{0}</b> : Oh, Gott verdammt. Also mein Tag ist ruiniert. Der einzige Grund wieso ich immer noch hier her komme ist wegen der Pizza. Ich weiß nicht was es mit dieser Pizza auf sich hat, aber sie ist echt lecker. Thunfischauflauf andererseits ist eine grausame Abscheulichkeit die mit dem Rest von diesen Missgeburten hier eingesperrt sein sollte."},
            {"Intro_Convo3_4", "<b>{0}</b> : Uh, okay.."},

            {"Intro_Convo4_1", "<b>{0}</b> : Lass mich raten, du hast keine Freundin, oder?"},
            {"Intro_Convo4_2", "<b>{0}</b> : Uh, s-sprichst du mit mir?"},
            {"Intro_Convo4_3", "<b>{0}</b> : Klar sprech ich mit dir."},
            {"Intro_Convo4_4", "<b>{0}</b> : Ist es so offensichtlich?"},
            {"Intro_Convo4_5", "<b>{0}</b> : Also ich bin ja kein Gedankenleser, sonst wäre ich auch hier eingesperrt, also ja, es ist ziemlich offensichtlich."},
            {"Intro_Convo4_6", "<b>{0}</b> : Wie soll ich denn mit diesem Job eine Freundin finden? Ich meine, ich kann ihr davon ja nichts erzählen, also was soll ich tun?"},
            {"Intro_Convo4_7", "<b>{0}</b> : Lüg sie doch einfach an. Sag ihr du arbeitest in irgendeinem Café oder so."},
            {"Intro_Convo4_8", "<b>{0}</b> : Aber, was wenn ich aus Versehen vergesse hier meine Hände zu waschen und mit Blut an ihnen nach Hause komme? Was sollte ich ihr dann sagen?"},
            {"Intro_Convo4_9", "<b>{0}</b> : Uh, keine Ahnung, sag einfach es wäre, uh... Ketchup oder so."},
            {"Intro_Convo4_10", "<b>{0}</b> : Ketchup? Wieso sollte ich Ketchup an meinen Händen haben wenn ich in einem Café arbeite?"},
            {"Intro_Convo4_11", "<b>{0}</b> : Ugh, v-vergiss es einfach."},

            {"Intro_Convo5_1", "<b>{0}</b> : Uh, hast du in letzter Zeit irgendwelche guten Filme gesehen?"},
            {"Intro_Convo5_2", "<b>{0}</b> : Uh, ich schaue nicht wirklich viele Filme."},
            {"Intro_Convo5_3", "<b>{0}</b> : Oh ok. Wie sieht es mit Videospielen aus? Weißt du was, das gibt mir eine Idee, jemand sollte ein Videospiel über diesen Ort machen."},
            {"Intro_Convo5_4", "<b>{0}</b> : Wieso sollte das irgendjemand tun?"},
            {"Intro_Convo5_5", "<b>{0}</b> : Ich weiß nicht, ich dachte einfach es wäre eine coole Idee."},
            {"Intro_Convo5_6", "<b>{0}</b> : Nun, ich spiele auch keine Videospiele."},



            {"Escort1", "<b>{0}</b> : Folge mir. Ach und übrigens, wir sind dazu autorisiert unfolgsame Testsubjekte zu neutralisieren, also versuch besser nichts Dummes."},
            {"Escort2", "<b>{0}</b> : Folge mir. Ach und übrigens, wir sind dazu autorisiert unfolgsame Testsubjekte zu neutralisieren, also versuch besser nichts Dummes."},
            {"EscortDone1", "<b>{0}</b> : Also, hier sind wir. Geh einfach da rein und befolge alle Anweisungen, dann wirst du uh, wahrscheinlich lebend da rauskommen."},
            {"EscortDone2", "<b>{0}</b> : Also, hier sind wir. Ich bin immer noch enttäuscht, dass ich dir nicht Eine reinhauen konnte, aber egal."},
            {"EscortDone3", "<b>{0}</b> : Also, hier sind wir. Ich bin immer noch enttäuscht, dass ich dir nicht ins Gesicht schlagen durfte, aber *seufz* egal."},
            {"EscortDone4", "<b>{0}</b> : Geh einfach da rein und befolge alle Anweisungen, dann wirst du uh, wahrscheinlich lebend da rauskommen. Vielleicht aber auch nicht, ist mir eigentlich auch egal."},
            {"EscortDone5", "<b>{0}</b> : Naja, jedenfalls, lass uns keine Zeit mehr verschwenden. Die Anderen warten auf dich in der Kammer."},


            {"EnterChamber","<b>{0}</b> : Achtung an alle D-Klasse Mitarbeiter, bitte betreten Sie die Isolierungszelle." },
            {"Approach173","<b>{0}</b> : Bitte nähern sie sich <color=yellow>SCP-173</color> für einen Versuch." },
            { "Problem","<b>{0}</b> : Uh, es scheint ein Problem mit dem Türenkontrollsystem zu geben, die Türen reagieren nicht auf unsere Versuche sie zu schließen, also uhm, bitte halten sie mit Blickkontakt mit SCP-173 und-" },
            { "Refuse1", "<b>{0}</b> : Subjekt D-9341, betreten sie die Isolierungszelle oder sie werden neutralisiert."  },
            { "Refuse2", "<b>{0}</b> : Dies ist ihre letzte Warnung. Sie haben fünf Sekunden um den Anweisungen zu folgen." },
            { "Refuse3", "<b>{0}</b> : Subjekt D-9341 zum Abschuss freigegeben. Feuer frei." },


            { "Escape1","<b>{0}</b> : Ich denke der kürzeste Weg nach draußen ist durch den Südost-Flügel, folgen Sie mir!" },
            { "Escape2","<b>{0}</b> : Haben Sie das gehört? Ich hoffe es war nicht-" },






            {"scene_BreachStart_1", "<b>{0}</b> : Agent, hinter dir!"},
            {"scene_BreachStart_2", "<b>{0}</b> : Oh, Scheiße!"},
            {"scene_BreachStart_3", "<b>{0}</b> : Behalt ihn im Auge!"},
            {"scene_BreachStart_4", "<b>{0}</b> : Okay, ich werde blinzeln, schau ihn einfach weiter an"},
            {"scene_BreachStart_5", "<b>{0}</b> : Okay, Alles klar"},


            {"scene_012_1", "<i>Ich muss... Ich muss es fertig stellen...</i>"},
            {"scene_012_2", "<i>Möchtest du das wirklich tun... Ich schaffe... das... nicht.</i>"},
            {"scene_012_3", "<i>Ich... Ich... muss... es schaffen.</i>"},
            {"scene_012_4", "<i>I-Ich... habe... keine... W-Wahl!</i>"},
            {"scene_012_5", "<i>Luftballons... Das....das macht...keinen Sinn!</i>"},
            {"scene_012_6", "<i>Nein... das... das kann... nicht sein!</i>"},
            {"scene_012_7", "<i>Es ist... Es ist unmöglich!</i>"},

            { "kneel106", "KNIE NIEDER"},
        }
    },
    {"CH", new Dictionary<string, string>() {
            {"BeforeDoorOpen", "<b>{0}</b> : 管理中心，我是尤格林探员。我需要打开3-11号房间。"},
            {"ExitCell", "<b>{0}</b> : 嘿，他们为你找了些工作。帮个忙，离开你的房间"},
            {"ExitCellRefuse1", "<b>{0}</b> : 你是傻瓜还是怎么了？我说离开你的房间。如果你不离开我就会踢你的屁股"},
            {"ExitCellRefuse2", "<b>{0}</b> : 听着，伙计，我不是一整天都有时间。我想礼貌一点。如果你不离开房间，我就踢你屁股。"},
            {"CellGas1", "<b>{0}</b> : 啊，你是这里有史以来最笨的测试对象了。哦，好吧，关上门，打开气阀。"},
            {"CellGas2", "<b>{0}</b> : 啊，我真的有点失望，你没被打。我还期待着打你的脸。"},
            {"EscortRun", "<b>{0}</b> : 嘿，笨蛋！你走错了路。马上过来！"},
            {"EscortRefuse1", "<b>{0}</b> : 快点！他们在等你。"},
            {"EscortRefuse2", "<b>{0}</b> : 听着，伙计，我已经讨厌我的工作了。你为什么让我更难做？"},
            {"EscortPissedOff1", "<b>{0}</b> : 我没心情和你胡扯，如果你不合作的话，我会对着你的脑子开一枪。"},
            {"EscortPissedOff2", "<b>{0}</b> : 我没心情和你胡扯，如果你不合作的话，我会对着你的脑子开一枪。"},
            {"EscortKill1", "<b>{0}</b> : 好吧，听我说。好吧，就这样吧。我们得去找其他人了"},
            {"EscortKill2", "<b>{0}</b> : 好吧，就这样。那我们就去找别人吧。"},


            {"Intro_Convo1_1", "<b>{0}</b> : 那么，呃，最近怎么样？"},
            {"Intro_Convo1_2", "<b>{0}</b> : 呃，你-你在和我说话吗？"},
            {"Intro_Convo1_3", "<b>{0}</b> : 嗯，是的，你觉得我在跟谁说话，这个长着要挨打的脸的家伙？我当然在和你说话。"},
            {"Intro_Convo1_4", "<b>{0}</b> : 哦，我只是有点惊讶。我想这是你第一次和我说话。"},
            {"Intro_Convo1_5", "<b>{0}</b> : 是的，这是你在这里工作的第一天。"},
            {"Intro_Convo1_6", "<b>{0}</b> : 实际上，我们已经在一起工作了5个月了。"},
            {"Intro_Convo1_7", "<b>{0}</b> : 真的吗？好奇怪啊。"},

            {"Intro_Convo2_1", "<b>{0}</b> : 呃，你最近看了什么好电影吗？"},
            {"Intro_Convo2_2", "<b>{0}</b> : 呃，我真的不看电影。我主要看书。"},
            {"Intro_Convo2_3", "<b>{0}</b> : 是吗？那看的是什么书？"},
            {"Intro_Convo2_4", "<b>{0}</b> : 呃，恐怖小说，科幻小说之类的？"},
            {"Intro_Convo2_5", "<b>{0}</b> : 你在开玩笑吧。"},
            {"Intro_Convo2_6", "<b>{0}</b> : 什么？"},
            {"Intro_Convo2_7", "<b>{0}</b> : 你的整个工作都围绕着恐怖和科幻小说展开，其实，你知道，这不是虚构的。"},
            {"Intro_Convo2_8", "<b>{0}</b> : 实际上，我正计划写一本关于我的经验的书-"},
            {"Intro_Convo2_9", "<b>{0}</b> : 是的，听着，没有冒犯的意思，但我已经对你所说的失去了兴趣。"},

            {"Intro_Convo3_1", "<b>{0}</b> : 伙计，我饿了。嘿，今天的披萨日在自助餐厅，对吧？"},
            {"Intro_Convo3_2", "<b>{0}</b> : 嗯，事实上，我想是金枪鱼砂锅。"},
            {"Intro_Convo3_3", "<b>{0}</b> : 哦，天哪。好吧，我的一天被毁了。我来这里的唯一原因是吃披萨。我不知道那比萨为什么那么美味。另一方面，金枪鱼砂锅是一种恶心的可憎的食物，它应该和其他的怪物一起关在这里。"},
            {"Intro_Convo3_4", "<b>{0}</b> : 好吧.."},

            {"Intro_Convo4_1", "<b>{0}</b> : 让我猜猜。你没有女朋友，对吧？"},
            {"Intro_Convo4_4", "<b>{0}</b> : 那很明显吗？"},
            {"Intro_Convo4_5", "<b>{0}</b> : 嗯哼，我绝对不是一个读心术的人，否则我会被关在这个地方，所以，是的，我会说这很明显。"},
            {"Intro_Convo4_6", "<b>{0}</b> : 好吧，当我有这份工作的时候，我该怎么找女朋友呢？我是说，我不能告诉她，那我该怎么办？"},
            {"Intro_Convo4_7", "<b>{0}</b> : 对她撒谎。告诉她你在咖啡店或别的什么地方工作。"},
            {"Intro_Convo4_8", "<b>{0}</b> : 好吧，如果我在工作时不小心忘了洗手，然后我回到家，手上沾满了血怎么办？那我对她说什么？"},
            {"Intro_Convo4_9", "<b>{0}</b> : 呃，我不知道，告诉她，呃…番茄酱。"},
            {"Intro_Convo4_10", "<b>{0}</b> : 番茄酱？如果我在咖啡店工作，为什么我会手上会有番茄酱？"},
            {"Intro_Convo4_11", "<b>{0}</b> : 呃，忘-忘了它吧。"},

            {"Intro_Convo5_1", "<b>{0}</b> : 呃，你最近看了什么好电影吗？"},
            {"Intro_Convo5_2", "<b>{0}</b> : 呃，我真的不看电影。"},
            {"Intro_Convo5_3", "<b>{0}</b> : 哦，好的。那么电子游戏呢？你知道这提醒了我，有人应该在这个地方做一个电子游戏。"},
            {"Intro_Convo5_4", "<b>{0}</b> : 为什么会有人这么做？"},
            {"Intro_Convo5_5", "<b>{0}</b> : 我不知道，只是觉得这个主意很酷。"},
            {"Intro_Convo5_6", "<b>{0}</b> : 嗯，我也不玩电子游戏。"},



            {"Escort1", "<b>{0}</b> : 跟着我。哦，顺便说一下，我们可以杀死任何不听话的测试对象，所以不要尝试任何愚蠢的事情。"},
            {"Escort2", "<b>{0}</b> : 跟着我。哦，顺便说一下，我们可以杀死任何不听话的测试对象，所以不要尝试任何愚蠢的事情。"},
            {"EscortDone1", "<b>{0}</b> : 好吧，我们到了。你只要进去按照指示去做，嗯，你可能会没事的。"},
            {"EscortDone2", "<b>{0}</b> : 好吧，我们到了。我还是很失望我没打你，但不管怎样。"},
            {"EscortDone3", "<b>{0}</b> : 好吧，我们到了。我还是很失望没能打你的脸，但*叹气*不管怎么样。"},
            {"EscortDone4", "<b>{0}</b> : 你只要进去按照指示去做，嗯，你可能会好起来的。或许你不会。不管怎样，我真的不在乎。"},
            {"EscortDone5", "<b>{0}</b> : 不管怎样，我们不要再浪费时间了。他们在房间里等你。"},


            {"EnterChamber","<b>{0}</b> : 所有D级人员注意，请进入收容室" },
            {"Approach173","<b>{0}</b> : 请接近<color=yellow>SCP-173</color>来进行测试" },
            { "Problem","<b>{0}</b> : 呃，看起来门控系统有点问题，我们关闭门的尝试都没有奏效，所以呃，请继续保持直视SCP-173并且-" },

            { "Escape1","<b>{0}</b> : 我猜最短的出口，就是通过东南门，跟我来！" },
            { "Escape2","<b>{0}</b> : 你听到了吗？我希望那不是-" },

            { "Refuse1", "<b>{0}</b> : 项目D-9341,进入收容间或你被消灭"  },
            { "Refuse2", "<b>{0}</b> : 这是你最后的警告。你有五秒钟的时间。" },
            { "Refuse3", "<b>{0}</b> : 项目D-9341任务结束，随意开火" },

            { "kneel106", "跪下"},






            {"scene_BreachStart_1", "<b>{0}</b> : 特工！在你后面"},
            {"scene_BreachStart_2", "<b>{0}</b> : 卧槽！"},
            {"scene_BreachStart_3", "<b>{0}</b> : 盯着他！"},
            {"scene_BreachStart_4", "<b>{0}</b> : 好了，我要眨眼了，盯着他"},
            {"scene_BreachStart_5", "<b>{0}</b> : 好的，明白了"},


            {"scene_012_1", "<i>我必须...我必须完成它...</i>"},
            {"scene_012_2", "<i>你真的想这样做吗…我不...想...我能做到。</i>"},
            {"scene_012_3", "<i>我...我...必须…做。</i>"},
            {"scene_012_4", "<i>我-我...没...有...选-选择！</i>"},
            {"scene_012_5", "<i>脑袋疼...这...说...不通!</i>"},
            {"scene_012_6", "<i>不...这...这是...不可能的</i>"},
            {"scene_012_7", "<i>这不可能...这不可能被完成</i>"},
        }
    },
};












































    //~~~~~~~~~~~~~~~~~~~~~~~~~~UI ENGLISH STRINGS~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static Dictionary<string, string> uiStrings_EN = new Dictionary<string, string>()
    {
        {"ui_generic_back", "Back"},
        {"ui_main_play", "Play"},
        {"ui_main_extras", "Extras"},
        {"ui_main_options", "Options"},
        {"ui_main_exit", "Exit"},

        {"ui_main_play_pro", "Prologue"},
        {"ui_main_play_chap", "Chapters"},

        {"ui_main_play_new", "New Game"},
        {"ui_main_play_load", "Load Game"},
        {"ui_main_play_start", "Start"},
        {"ui_main_play_seed", "Map Generation Seed"},
        {"ui_main_play_sname", "Savefile Name"},
        {"ui_main_play_intro", "Play Intro"},
        {"ui_main_play_sload", "Load"},
        {"ui_main_play_sdelete", "Delete"},

        {"ui_in_anykey", "Press any key to Start"},
        {"ui_in_pause", "PAUSE"},
        {"ui_in_info", "Designation: {0} \nName: {1} \n\nSave File: {2}\nMap Seed: {3}"},
        {"ui_in_resume", "Resume"},
        {"ui_in_save", "Save and Quit"},
        {"ui_in_quit", "Quit"},
        {"ui_in_death", "YOU DIED"},
        {"ui_in_load", "Load Save"},
        {"ui_in_saved", "Game Saved" },
        {"ui_in_nosave", "Can't save at this location" },


        {"ui_op_gfx", "Graphics"},
        {"ui_op_sfx", "Audio"},
        {"ui_op_input", "Input"},
        {"ui_op_misc", "Advanced"},

        {"ui_gfx_quality", "Graphics Quality"},
        {"ui_gfx_post", "Post Processing Quality"},
        {"ui_gfx_lang", "Language"},
        {"ui_gfx_vsync", "V-Sync"},
        {"ui_gfx_frame", "Set Framerate Target"},
        {"ui_gfx_gamma", "Screen Gamma"},

        {"ui_sfx_master", "Master Volume"},
        {"ui_sfx_music", "Music"},
        {"ui_sfx_ambiance", "Ambiance"},
        {"ui_sfx_sfx", "Sound Effects"},
        {"ui_sfx_voice", "Voice"},
        {"ui_sfx_sub", "Show Subtitles"},

        {"ui_input_invert", "Invert Mouse Y-Axis"},
        {"ui_input_acc", "Mouse Acceleration"},

        {"ui_misc_debug", "Debug Console"},
        {"ui_misc_tuto", "Show In-game Tutorials"},

        {"ui_demo_end", "Thanks for Playing the our Demo" },
        {"ui_demo_end_body", "Our first objective with this remaster is to replicate the same ambiance and feeling from the original <color=white><i>SCP: Containment Breach</i></color> in a more detailed rendering engine, and a (hopefully!) more reliable engine.\n\nWe hope you think we have delivered!\n\nWe have a long way too go to reach feature parity with the current version. However as a creator, you have that itch to both perfect your work, but also get in on the hands of the people who will enjoy it. I believe I have turned this engine into something that will deliver a definitive SCP:CB experience quick and in high quality.\n\nYou can keep playing and explore the work we have in the Heavy Containment Zone by reloading this save.\n\nKeep an eye out for the next major update! Hopefully there you'll have a taste of the new features and ideas we want to build into the main game." },

        {"ui_map_noconnect", "UNABLE TO CONNECT TO MAP DATABASE" },
        {"ui_map_lock", "LOCKDOWN IN AREAS: " },
        {"ui_map_heavy", "HEAVY CONT. ZONE " },
        {"ui_map_entrance", "ENTRANCE ZONE " },

        {"ui_radio_channel2", " WARNING - CONTAINMENT BREACH " },
        {"ui_radio_channel3", " SCP Foundation On-Site Radio " },
        {"ui_radio_channel4", " EMERGENCY CHANNEL - RESERVED FOR COMMUNICATION IN THE EVENT OF A CONTAINMENT BREACH " },



    };

    public static Dictionary<string, string> loadStrings_EN = new Dictionary<string, string>()
    {
        {"title_173", "SCP-173"},
        {"body1_173", "SCP-173 is constructed from concrete and rebar with traces of Krylon brand spray paint. It is animate and extremely hostile."},
        {"body2_173", "The object cannot move while within a direct line of sight. Line of sight must not be broken at any time with SCP-173. Personnel assigned to enter container are instructed to alert one another before blinking."},

        {"title_scp", "The SCP Foundation"},
        {"body1_scp", "The SCP Foundation is an organization dedicated to the containment and research of anomalous artifacts and lifeforms."},
        {"body2_scp", "''SCP'' stands for ''Special Containment Procedures'' (and the official motto of the foundation, ''Secure, Contain, Protect''.) - which sums up both the goals and methods of the Foundation's actions."},

        {"title_dclass", "Class-D Personnel"},
        {"body1_dclass", "Class-D personnel are designated staff used to handle the Keter-level objects."},
        {"body2_dclass", "Class-D personnel are recruited from prison inmates. Condemned persons are preferred; in times of duress, Protocol 12 can be authorized, allowing recruitment of innocents or persons incarcerated for lesser crimes."},
        {"body3_dclass", "All Class-D personnel are to be terminated at the first of the month, and a new staff must be ready to replace them."},

        {"title_012", "SCP-012" },
        {"body1_012" , "SCP-012 was retrieved by Archaeologist K.M. Sandoval during the excavation of a northern Italian tomb. The object, a piece of handwritten musical score entitled \"On Mount Golgotha\", appears to be incomplete."},
        {"body2_012" , "The red/black ink, first thought to be some form of berry or natural dye ink, was later found to be human blood from multiple subjects." },
        {"body3_012" , "Multiple test subjects have been allowed access to the score. In every case, the subjects mutilated themselves in order to use their own blood to finish the piece, resulting in subsequent symptoms of psychosis and massive trauma." },

        {"title_106", "SCP-106" },
        {"body1_106","SCP-106 appears to be an elderly humanoid, with a general appearance of advanced decomposition. This appearance may vary, but the \"rotting\" quality is observed in all forms." },
        {"body2_106", "SCP-106 causes a \"corrosion\" effect in all solid matter it touches, engaging a physical breakdown in materials several seconds after contact. This is observed as rusting, rotting, and cracking of materials, and the creation of a black, mucus-like substance similar to the material coating SCP-106." },
        {"body3_106","SCP-106 can pass through solid matter and will capture and kill its prey by pulling it into what is assumed to be its personal \"pocket dimension\"."},

        {"title_294","SCP-294" },
        {"body1_294", "Item SCP-294 appears to be a standard coffee vending machine, the only noticeable difference being an entry touchpad with buttons corresponding to an English QWERTY keyboard." },
        {"body2_294", "Upon entering the name of any liquid using the touchpad, a standard 12-ounce paper drinking cup is placed and the liquid indicated is poured." },
        {"body3_294", "Ninety-seven initial test runs were performed (including requests for water, coffee, beer, and soda, non-consumable liquids such as sulfuric acid, wiper fluid, and motor oil, as well as substances that do not usually exist in liquid state, such as nitrogen, iron and glass) and each one returned a success." },

        {"title_914", "SCP-914" },
        {"body1_914" , "SCP-914 is a large clockwork device weighing several tons and covering an area of eighteen square meters, consisting of screw drives, belts, pulleys, gears, springs and other clockwork." },
        {"body2_914","When an object is placed in the Intake Booth and the key is wound up, SCP-914 will \"refine\" the object. No energy is lost in the process, and the object is refined based on the setting specified on 914's front panel." },
        {"body3_914","No organic matter is to be entered in to SCP-914 at any time." },

        {"title_939", "SCP-939" },
        {"body1_939","SCP-939 are endothermic, pack-based predators which display atrophy of various systems similar to troglobitic organisms. SCP-939 average 2.2 meters tall standing upright and weigh an average of 250 kg, though weight is highly variable." },
        {"body2_939","SCP-939's primary method of luring prey is the imitation of human speech in the voices of prior victims, though imitation of other species and active nocturnal hunts have been documented." },
        {"body3_939","Prey is usually killed with a single bite to the cranium or neck; bite forces have been measured in excess of 35 MPa." },


    };

    public static Dictionary<string, string> tutoStrings_EN = new Dictionary<string, string>()
    {
        {"tutograb", "Click the Interact button when the Hand icon shows up to interact with different objects. Grab Items, move lever, etc."},
        {"tutoinv1", "Press the Inventory key to see your collected items. Click on the to equip them or use them"},
        {"tutoinv2", "Drop items into the slots of other items to combine them for different results. You can also drop them outside"},
        {"tutoinv3", "Card Readers around the facility require a keycard with the appropiate clearance. Some doors may be locked remotely. Find ways to unlock them"},
        {"tutodead", "You may find interesting items in the pockets of the victims of the breach"},
        {"tutorun", "Hold the sprint button to Sprint. Be aware your footsteps may attract creatures aware of their enviroment"},
        {"tutohide1", "Crouching and hiding behind objects can deter Enemies aware of their enviroment"},
        {"tutohide2", "Creatures can hear your footsteps. Crouch or walk slowly if you think they are aware of your presence"},
        {"tutoradio", "Press keys 1 to 5 to change the Radio Channel"},
        };

    public static Dictionary<string, string> playStrings_EN = new Dictionary<string, string>()
    {
        {"play_button_nocard", "You need a Keycard for this door"},
        {"play_button_lowcard", "You need a Keycard with a Higher clearance"},
        {"play_button_card", "You slide the Keycard in the slot"},
        {"play_button_failcard", "You slide the Keycard but nothing happened"},
        {"play_button_elev", "You called the Elevator"},
        {"play_button", "You pushed the button"},
        {"play_equip_fem", "You put on the {0}" },
        {"play_equip_male", "You put on the {0}" },
        {"play_equip_uni", "You put on {0}" },
        {"play_dequip_fem", "You removed the {0}" },
        {"play_dequip_male", "You removed the {0}" },
        {"play_dequip_uni", "You removed {0}" },
        {"play_used_fem", "You used the {0}" },
        {"play_used_male", "You used the {0}" },
        {"play_used_uni", "You used {0}" },
        {"play_picked_uni", "Picked up {0}" },
        {"play_picked_fem", "Picked up a {0}" },
        {"play_picked_male", "Picked up a {0}" },
        {"play_equiped", "Using {0}" },
        {"play_fullinv", "Can't carry more items" },
        {"play_cure", "You patch yourself up" },
        {"play_cureblood", "You patch yourself up a bit, and stop the bloodloss" },
        {"play_cureblood2", "You patch yourself up, but didnt managed to stop the bloodloss completely" },

        {"play_012_1", "You start pushing your nails into your wrist, drawing blood." },
        {"play_012_2", "You tear open your left wrist and start writing on the composition with your blood." },
        {"play_012_3", "You push your fingers deeper into the wound." },
        {"play_012_4", "You rip the wound wide open. Grabbing scoops of blood pouring out." },


    };

    public static Dictionary<string, string> itemStrings_EN = new Dictionary<string, string>()
    {
        {"bat_nor", "9V Battery"},
        {"doc_ori", "Orientation Leaflet"},
        {"doc_173", "173 Containment Procedures"},
        {"doc012", "SCP-012 Containment Procedures"},
        {"doc079", "SCP-079 Containment Procedures"},
        {"doc096", "SCP-096 Containment Procedures"},
        {"doc682", "SCP-682 Containment Procedures"},
        {"doc939", "SCP-939 Containment Procedures"},
        {"doc966", "SCP-966 Containment Procedures"},
        {"doc1048", "SCP-1048 Containment Procedures"},
        {"doc1123", "SCP-1123 Containment Procedures"},
        {"doc372", "SCP-372 Containment Procedures"},
        {"doc914", "SCP-914 Document"},

        {"docAC", "Burned Note"},

        {"docL1", "Dr. L Note"},

        {"doc500", "SCP-500 Document"},
        {"docSC", "Security Clearance Document"},
        {"docRAND3", "173 Procedures Revision"},


        {"deadpaper", "Shreded paper"},
        {"origami", "Origami"},
        {"clipboard", "Clipboard"},
        {"elec", "Destroyed Electronics"},

        {"gasmask1", "Gas Mask"},
        {"gasmask2", "Gas Mask"},
        {"eye", "Eye Drops"},
        {"vest", "Ballistic Vest"},
        {"key0", "Janitor Keycard"},
        {"key1", "Researcher Keycard LVL 1"},
        {"key2", "Researcher Keycard LVL 2"},
        {"key3", "Researcher Keycard LVL 3"},
        {"key4", "Agent Keycard LVL 4"},
        {"key5", "Agent Keycard LVL 5"},
        {"badge", "Badge"},
        {"keycredit", "Master Card"},
        {"keyj", "Playing Card"},
        {"keyomni", "OmniCard"},
        {"medkit1", "Light MedKit"},
        {"ring", "SCP-714"},
        {"snav", "S-Nav 300"},
        {"snav2", "S-Nav 310"},
        {"snav3", "S-Nav Ultimate"},
        {"radio", "Radio"},




    };

    public static Dictionary<string, string> sceneStrings_EN = new Dictionary<string, string>()
    {
        {"BeforeDoorOpen", "<b>{0}</b> : Control, this is Agent Ulgrin. I need to request open up Cell 3-11."},
        {"ExitCell", "<b>{0}</b> : Hey, they've got some work for ya. Do me a favor, and step out of your cell"},
        {"ExitCellRefuse1", "<b>{0}</b> : What are you stupid or something? I said step out of the cell. If you don't step out of the cell I'm gonna kick your ass."},
        {"ExitCellRefuse2", "<b>{0}</b> : Look buddy I don't have all day. I'm trying to be polite about this. If you don't step out of the cell I'm gonna kick your ass."},
        {"CellGas1", "<b>{0}</b> : Huh, you have got to be the dumbest test subject we've ever had. Oh well, shut the doors and open the gas valves."},
        {"CellGas2", "<b>{0}</b> : Huh, I'm actually kinda disappointed you didn't put up a fight. I was looking forward to punching you in the face."},
        {"EscortRun", "<b>{0}</b> : Hey, dumbass! You're going the wrong way. Get the hell over here right now!"},
        {"EscortRefuse1", "<b>{0}</b> : Hurry up! They're waiting for you."},
        {"EscortRefuse2", "<b>{0}</b> : Look dude, I already hate my job. Why are you making it more difficult for me?"},
        {"EscortPissedOff1", "<b>{0}</b> : I ain't in the mood for this shit, I have no problem putting a bullet in your brain if you don't start cooperating."},
        {"EscortPissedOff2", "<b>{0}</b> : I ain't in the mood for this shit, I have no problem putting a bullet in your brain if you don't start cooperating."},
        {"EscortKill1", "<b>{0}</b> : Alright, you know what? Fine, be that way. We'll just get somebody else then"},
        {"EscortKill2", "<b>{0}</b> : Alright fine, be that way. We'll just get somebody else then."},


        {"Intro_Convo1_1", "<b>{0}</b> : So, uh, how's it going?"},
        {"Intro_Convo1_2", "<b>{0}</b> : Uh, a-are you talking to me?"},
        {"Intro_Convo1_3", "<b>{0}</b> : Well yeah, who do you think I'm talking to, this guy with the punchable face? Course I'm talking to you."},
        {"Intro_Convo1_4", "<b>{0}</b> : Oh, I'm just a little surprised. I think this is the first time you've ever spoken to me."},
        {"Intro_Convo1_5", "<b>{0}</b> : Well yeah, it's your first day working here."},
        {"Intro_Convo1_6", "<b>{0}</b> : Uh, actually, we've worked together now for about 5 months."},
        {"Intro_Convo1_7", "<b>{0}</b> : Really? Wow. That's weird."},

        {"Intro_Convo2_1", "<b>{0}</b> : Uh, so you see any good movies lately?"},
        {"Intro_Convo2_2", "<b>{0}</b> : Uh, I don't really watch movies. I mostly read books."},
        {"Intro_Convo2_3", "<b>{0}</b> : Yeah? What kind of books?"},
        {"Intro_Convo2_4", "<b>{0}</b> : Uh, horror, science fiction, anything like that?"},
        {"Intro_Convo2_5", "<b>{0}</b> : You're kidding me."},
        {"Intro_Convo2_6", "<b>{0}</b> : What?"},
        {"Intro_Convo2_7", "<b>{0}</b> : Your whole job revolves around horror and science fiction, except, you know, it's not actually fiction."},
        {"Intro_Convo2_8", "<b>{0}</b> : Well, actually, I'm planning on writing a book about my exper-"},
        {"Intro_Convo2_9", "<b>{0}</b> : Yeah, look, no offense, but I've already lost interest in what you're talking about."},

        {"Intro_Convo3_1", "<b>{0}</b> : Man, I'm hungry. Hey, today's pizza day down at the cafeteria, right?"},
        {"Intro_Convo3_2", "<b>{0}</b> : Uh, a-actually, I think it's tuna casserole."},
        {"Intro_Convo3_3", "<b>{0}</b> : Oh, god dammit. Well, my day's ruined. The only reason I still come here is for the pizza. I don't know what it is about that pizza, but it's delicious. Tuna casserole, on the other hand, is a disgusting abomination and it should be locked up in here with the rest of these freaks."},
        {"Intro_Convo3_4", "<b>{0}</b> : Uh, okay.."},

        {"Intro_Convo4_1", "<b>{0}</b> : Let me guess. You don't have a girlfriend, do you?"},
        {"Intro_Convo4_2", "<b>{0}</b> : Uh, a-are you talking to me?"},
        {"Intro_Convo4_3", "<b>{0}</b> : Course I'm talking to you."},
        {"Intro_Convo4_4", "<b>{0}</b> : Is it that obvious?"},
        {"Intro_Convo4_5", "<b>{0}</b> : Well I'm definitely not a mind reader, otherwise I'd be locked up in this place, so, yeah, I'd say it's pretty damn obvious"},
        {"Intro_Convo4_6", "<b>{0}</b> : Well, how am I supposed to get a girlfriend anyway when I have this job? I mean, I can't tell her about it, so what am I supposed to do?"},
        {"Intro_Convo4_7", "<b>{0}</b> : Just lie to her. Tell her you work at some coffee shop or something."},
        {"Intro_Convo4_8", "<b>{0}</b> : Well, what if I accidentally forgot to wash my hands here at work, and I came home and there was blood on my hands? What would I say to her then?"},
        {"Intro_Convo4_9", "<b>{0}</b> : Uh, I don't know, tell her it's, uh... ketchup."},
        {"Intro_Convo4_10", "<b>{0}</b> : Ketchup? Why would I have ketchup on my hands if I worked at a coffee shop?"},
        {"Intro_Convo4_11", "<b>{0}</b> : Ugh, j-just forget it."},

        {"Intro_Convo5_1", "<b>{0}</b> : Uh, so you see any good movies lately?"},
        {"Intro_Convo5_2", "<b>{0}</b> : Uh, I don't really watch movies."},
        {"Intro_Convo5_3", "<b>{0}</b> : Oh ok. Well what about video games? You know that reminds me, someone should make a video game based on this place."},
        {"Intro_Convo5_4", "<b>{0}</b> : Why would anyone do that?"},
        {"Intro_Convo5_5", "<b>{0}</b> : I don't know, just thought it was kind of a cool idea."},
        {"Intro_Convo5_6", "<b>{0}</b> : Well, I don't play video games either."},



        {"Escort1", "<b>{0}</b> : Just follow me. Oh and by the way, we're authorized to kill any disobedient test subjects, so don't try anything stupid."},
        {"Escort2", "<b>{0}</b> : Just follow me. Oh and by the way, we're authorized to kill any disobedient test subjects, so don't try anything stupid."},
        {"EscortDone1", "<b>{0}</b> : Well, we're here. Just get in there and follow all the instructions and uh, you'll probably be fine."},
        {"EscortDone2", "<b>{0}</b> : Well, we're here. I'm still disappointed I didn't get to punch you, but whatever."},
        {"EscortDone3", "<b>{0}</b> : Well, we're here. I'm still disappointed I didn't get to punch you in the face, but *sigh* whatever."},
        {"EscortDone4", "<b>{0}</b> : Just get in there and follow all the instructions and uh, you'll probably be fine. Or maybe you won't be. Either way, I don't really care."},
        {"EscortDone5", "<b>{0}</b> : Well anyway, let's not waste anymore time. They're waiting for you down in the chamber."},


        {"EnterChamber","<b>{0}</b> : Attention all Class D Personel, Please enter the Containment Chamber" },
        {"Approach173","<b>{0}</b> : Please approach <color=yellow>SCP-173</color> for testing" },
        { "Problem","<b>{0}</b> : Uh, there seems to be a problem with the door control system,  the doors aren't responding to any of our attempts to close it, so Uhm, Please maintain direct eye contact with SCP-173 and-" },
        { "Refuse1", "<b>{0}</b> : Subject D-9341, enter the containment chamber or you will be terminated."  },
        { "Refuse2", "<b>{0}</b> : This is your last warning. You have five seconds to comply." },
        { "Refuse3", "<b>{0}</b> : Subject D-9341 designated for termination. Fire at will." },



        { "Escape1","<b>{0}</b> : I think the shortest way out, is through the South-east wing, follow me!" },
        { "Escape2","<b>{0}</b> : Did you hear that? I hope it wasnt-" },






        {"scene_BreachStart_1", "<b>{0}</b> : Agent, Behind You!"},
        {"scene_BreachStart_2", "<b>{0}</b> : Oh, shit!"},
        {"scene_BreachStart_3", "<b>{0}</b> : Keep your eyes on Him!"},
        {"scene_BreachStart_4", "<b>{0}</b> : Okay, I'm going to blink, just keep on watching him"},
        {"scene_BreachStart_5", "<b>{0}</b> : Alright, got it"},


        {"scene_012_1", "<i>I have to... I have to finish it...</i>"},
        {"scene_012_2", "<i>Do you really wanna do it... I don't... think... I can do this.</i>"},
        {"scene_012_3", "<i>I... I... must... do it.</i>"},
        {"scene_012_4", "<i>I-I... have... no... ch-choice!</i>"},
        {"scene_012_5", "<i>Balloons... This....this makes...no sense!</i>"},
        {"scene_012_6", "<i>No... this... this is... impossible!</i>"},
        {"scene_012_7", "<i>It can't... It can't be completed!</i>"},

        { "kneel106", "KNEEL"},
    };

    public static Dictionary<string, string> charaStrings_EN = new Dictionary<string, string>()
    {
        {"chara_franklin", "S.C. Franklin"},
        {"chara_ulgrin", "Agent Ulgrin"},
        {"chara_guard", "Guard"},
        {"chara_sci", "Doctor"},
    };

    public static Dictionary<string, string> deathStrings_EN = new Dictionary<string, string>()
    {
        {"death_173", "Subject D-9341: \nFatal cervical fracture. Assumed to be attacked by SCP-173."},
        {"death_173_doors", "\"<i>If I'm not mistaken, one of the main purposes of these rooms was to stop SCP-173 from moving further in the event of a containment breach. So, whose brilliant idea was it to put <b>A GODDAMN MAN-SIZED VENTILATION DUCT</b> in there?</i>\""},
        {"death_173_surv", "Subject: D-9341. \nCause of Death: Fatal cervical fracture. The surveillance tapes confirm that the subject was killed by SCP-173." },
        {"death_173_intro" , "Subject: D-9341. \nCause of death: Fatal cervical fracture. According to Security Chief Franklin who was present at SCP-173's containment chamber during the breach, the subject was killed by SCP-173 as soon as the disruptions in the electrical network started."},

        {"death_106_stone", "\" [...]<i>In addition to the decomposed appearance typical of the victims of SCP-106, the body exhibits injuries that have not been observed before: massive skull fracture, three broken ribs, fractured shoulder and heavy lacerations.</i>\"" },
        {"death_939" ,"\"[...] <i>All four escaped SCP-939 (4) specimens have been captured and recontained successfully. Three (3) of them made quite a mess at Storage Area 6. A cleaning team has been dispatched.</i> \""},
        {"death_012", "<i>Subject D-9341 found in a pool of blood next to SCP-012. Subject seems to have ripped open his wrists and written three extra lines to the composition before dying of blood loss.</i>" },


        {"death_intro",  "\"[...] <i>What an annoying piece of shit </i>\""},

        {"death_gas", "<i>Subject D-9341 found dead in [DATA REDACTED]. Cause of death: suffocation due to decontamination gas.</i>" },
        {"death_tesla", "Subject D-9341 killed by the Tesla Gate at [REDACTED]" },


        {"death_mtf", "Subject: D-9341. \nTerminated by Nine-Tailed Fox." },

    };
}
