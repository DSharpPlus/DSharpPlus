using System.Collections.Generic;

using NUnit.Framework;

namespace DSharpPlus.Tests.Interactivity;

public static class PageSplitterTestData
{
    public static readonly List<TestCaseData> lineWiseSplitting =
    [
        // single line
        new TestCaseData("Is this a dagger which I see before me,", new[] { "Is this a dagger which I see before me," }),

        // three lines
        new TestCaseData
        (
            """
            Is this a dagger which I see before me,
            The handle toward my hand? Come, let me clutch thee.
            I have thee not, and yet I see thee still.
            """, 
            new[] 
            {
                """
                Is this a dagger which I see before me,
                The handle toward my hand? Come, let me clutch thee.
                I have thee not, and yet I see thee still.
                """
            }
        ),

        // fifteen lines
        new TestCaseData
        (
            """
            Is this a dagger which I see before me,
            The handle toward my hand? Come, let me clutch thee.
            I have thee not, and yet I see thee still.
            Art thou not, fatal vision, sensible
            To feeling as to sight? or art thou but
            A dagger of the mind, a false creation,
            Proceeding from the heat-oppressed brain?
            I see thee yet, in form as palpable
            As this which now I draw.
            Thou marshall'st me the way that I was going;
            And such an instrument I was to use.
            Mine eyes are made the fools o' the other senses,
            Or else worth all the rest; I see thee still,
            And on thy blade and dudgeon gouts of blood,
            Which was not so before. There's no such thing:
            """, 

            new[] 
            { 
                """
                Is this a dagger which I see before me,
                The handle toward my hand? Come, let me clutch thee.
                I have thee not, and yet I see thee still.
                Art thou not, fatal vision, sensible
                To feeling as to sight? or art thou but
                A dagger of the mind, a false creation,
                Proceeding from the heat-oppressed brain?
                I see thee yet, in form as palpable
                As this which now I draw.
                Thou marshall'st me the way that I was going;
                And such an instrument I was to use.
                Mine eyes are made the fools o' the other senses,
                Or else worth all the rest; I see thee still,
                And on thy blade and dudgeon gouts of blood,
                Which was not so before. There's no such thing:
                """
            }
        ),

        // sixteen lines
        new TestCaseData
        (
            """
            Is this a dagger which I see before me,
            The handle toward my hand? Come, let me clutch thee.
            I have thee not, and yet I see thee still.
            Art thou not, fatal vision, sensible
            To feeling as to sight? or art thou but
            A dagger of the mind, a false creation,
            Proceeding from the heat-oppressed brain?
            I see thee yet, in form as palpable
            As this which now I draw.
            Thou marshall'st me the way that I was going;
            And such an instrument I was to use.
            Mine eyes are made the fools o' the other senses,
            Or else worth all the rest; I see thee still,
            And on thy blade and dudgeon gouts of blood,
            Which was not so before. There's no such thing:
            It is the bloody business which informs
            """, 

            new[] 
            {
                """
                Is this a dagger which I see before me,
                The handle toward my hand? Come, let me clutch thee.
                I have thee not, and yet I see thee still.
                Art thou not, fatal vision, sensible
                To feeling as to sight? or art thou but
                A dagger of the mind, a false creation,
                Proceeding from the heat-oppressed brain?
                I see thee yet, in form as palpable
                As this which now I draw.
                Thou marshall'st me the way that I was going;
                And such an instrument I was to use.
                Mine eyes are made the fools o' the other senses,
                Or else worth all the rest; I see thee still,
                And on thy blade and dudgeon gouts of blood,
                Which was not so before. There's no such thing:
                """,

                """
                It is the bloody business which informs
                """
            }
        ),

        // many lines
        new TestCaseData
        (
            """
            Is this a dagger which I see before me,
            The handle toward my hand? Come, let me clutch thee.
            I have thee not, and yet I see thee still.
            Art thou not, fatal vision, sensible
            To feeling as to sight? or art thou but
            A dagger of the mind, a false creation,
            Proceeding from the heat-oppressed brain?
            I see thee yet, in form as palpable
            As this which now I draw.
            Thou marshall'st me the way that I was going;
            And such an instrument I was to use.
            Mine eyes are made the fools o' the other senses,
            Or else worth all the rest; I see thee still,
            And on thy blade and dudgeon gouts of blood,
            Which was not so before. There's no such thing:
            It is the bloody business which informs
            Thus to mine eyes. Now o'er the one halfworld
            Nature seems dead, and wicked dreams abuse
            The curtain'd sleep; witchcraft celebrates
            Pale Hecate's offerings, and wither'd murder,
            Alarum'd by his sentinel, the wolf,
            Whose howl's his watch, thus with his stealthy pace.
            With Tarquin's ravishing strides, towards his design
            Moves like a ghost. Thou sure and firm-set earth,
            Hear not my steps, which way they walk, for fear
            Thy very stones prate of my whereabout,
            And take the present horror from the time,
            Which now suits with it. Whiles I threat, he lives:
            Words to the heat of deeds too cold breath gives.

            *A bell rings.*

            I go, and it is done; the bell invites me.
            Hear it not, Duncan; for it is a knell
            That summons thee to heaven or to hell.
            """, 

            new[] 
            {
                """
                Is this a dagger which I see before me,
                The handle toward my hand? Come, let me clutch thee.
                I have thee not, and yet I see thee still.
                Art thou not, fatal vision, sensible
                To feeling as to sight? or art thou but
                A dagger of the mind, a false creation,
                Proceeding from the heat-oppressed brain?
                I see thee yet, in form as palpable
                As this which now I draw.
                Thou marshall'st me the way that I was going;
                And such an instrument I was to use.
                Mine eyes are made the fools o' the other senses,
                Or else worth all the rest; I see thee still,
                And on thy blade and dudgeon gouts of blood,
                Which was not so before. There's no such thing:
                """,

                """
                It is the bloody business which informs
                Thus to mine eyes. Now o'er the one halfworld
                Nature seems dead, and wicked dreams abuse
                The curtain'd sleep; witchcraft celebrates
                Pale Hecate's offerings, and wither'd murder,
                Alarum'd by his sentinel, the wolf,
                Whose howl's his watch, thus with his stealthy pace.
                With Tarquin's ravishing strides, towards his design
                Moves like a ghost. Thou sure and firm-set earth,
                Hear not my steps, which way they walk, for fear
                Thy very stones prate of my whereabout,
                And take the present horror from the time,
                Which now suits with it. Whiles I threat, he lives:
                Words to the heat of deeds too cold breath gives.
                """,

                """
                *A bell rings.*

                I go, and it is done; the bell invites me.
                Hear it not, Duncan; for it is a knell
                That summons thee to heaven or to hell.
                """
            }
        )
    ];

    public static readonly List<TestCaseData> characterWiseSplitting =
    [
        // short
        new TestCaseData
        (
            "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet?",
            new[] 
            {
                "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet?"
            }
        ),

        // 500 characters
        new TestCaseData
        (
            "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet? quem ad finem sese effrenata iactabit audacia? Nihilne te nocturnum praesidium Palati, nihil urbis vigiliae, nihil timor populi, nihil concursus bonorum omnium, nihil hic munitissimus habendi senatus locus, nihil horum ora voltusque moverunt? Patere tua consilia non sentis, constrictam iam horum omnium scientia teneri coniurationem tuam non vides? Quid proxima, quid superiore nocte egeris, ubi fueris",
            new[] 
            {
                "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet? quem ad finem sese effrenata iactabit audacia? Nihilne te nocturnum praesidium Palati, nihil urbis vigiliae, nihil timor populi, nihil concursus bonorum omnium, nihil hic munitissimus habendi senatus locus, nihil horum ora voltusque moverunt? Patere tua consilia non sentis, constrictam iam horum omnium scientia teneri coniurationem tuam non vides? Quid proxima, quid superiore nocte egeris, ubi fueris"
            }
        ),

        // over 500 should split off at the end of the sentence or subclause
        new TestCaseData
        (
            "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet? quem ad finem sese effrenata iactabit audacia? Nihilne te nocturnum praesidium Palati, nihil urbis vigiliae, nihil timor populi, nihil concursus bonorum omnium, nihil hic munitissimus habendi senatus locus, nihil horum ora voltusque moverunt? Patere tua consilia non sentis, constrictam iam horum omnium scientia teneri coniurationem tuam non vides? Quid proxima, quid superiore nocte egeris, ubi fueris, quos convocaveris, quid consilii ceperis, quem nostrum ignorare arbitraris? O tempora, o mores! Senatus haec intellegit. Consul videt; hic tamen vivit. Vivit? immo vero etiam in senatum venit, fit publici consilii particeps, notat et designat oculis ad caedem unum quemque nostrum.",

            new[] 
            {
                "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet? quem ad finem sese effrenata iactabit audacia? Nihilne te nocturnum praesidium Palati, nihil urbis vigiliae, nihil timor populi, nihil concursus bonorum omnium, nihil hic munitissimus habendi senatus locus, nihil horum ora voltusque moverunt? Patere tua consilia non sentis, constrictam iam horum omnium scientia teneri coniurationem tuam non vides? Quid proxima, quid superiore nocte egeris, ubi fueris,",
                "quos convocaveris, quid consilii ceperis, quem nostrum ignorare arbitraris? O tempora, o mores! Senatus haec intellegit. Consul videt; hic tamen vivit. Vivit? immo vero etiam in senatum venit, fit publici consilii particeps, notat et designat oculis ad caedem unum quemque nostrum."
            }
        ),

        // unless it's over 600, then split off at the next available word
        // sorry, Marcus Tullius Cicero, i had to remove a subclause and a comma to align this
        new TestCaseData
        (
            "Meministine me ante diem XII Kalendas Novembris dicere in senatu fore in armis certo die, qui dies futurus esset ante diem VI Kal. Novembris, C. Manlium, audaciae satellitem atque administrum tuae? Num me fefellit, Catilina, non modo res tanta, tam atrox tamque incredibilis, verum dies? Dixi ego idem in senatu caedem te optumatium contulisse in ante diem V Kalendas Novembris, tum cum multi principes civitatis Roma non tam sui conservandi quam tuorum consiliorum reprimendorum causa profugerunt. Num infitiari potes te illo ipso die meis praesidiis mea diligentia circumclusum commovere te contra rem publicam non potuisse, cum tu discessu ceterorum nostra tamen, qui remansissemus, caede te contentum esse dicebas?",

            new[] 
            {
                "Meministine me ante diem XII Kalendas Novembris dicere in senatu fore in armis certo die, qui dies futurus esset ante diem VI Kal. Novembris, C. Manlium, audaciae satellitem atque administrum tuae? Num me fefellit, Catilina, non modo res tanta, tam atrox tamque incredibilis, verum dies? Dixi ego idem in senatu caedem te optumatium contulisse in ante diem V Kalendas Novembris, tum cum multi principes civitatis Roma non tam sui conservandi quam tuorum consiliorum reprimendorum causa profugerunt. Num infitiari potes te illo ipso die meis praesidiis mea diligentia circumclusum commovere te contra rem",
                "publicam non potuisse, cum tu discessu ceterorum nostra tamen, qui remansissemus, caede te contentum esse dicebas?"
            }
        ),

        // and now for the obligatory three-part split
        new TestCaseData
        (
            "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet? quem ad finem sese effrenata iactabit audacia? Nihilne te nocturnum praesidium Palati, nihil urbis vigiliae, nihil timor populi, nihil concursus bonorum omnium, nihil hic munitissimus habendi senatus locus, nihil horum ora voltusque moverunt? Patere tua consilia non sentis, constrictam iam horum omnium scientia teneri coniurationem tuam non vides? Quid proxima, quid superiore nocte egeris, ubi fueris, quos convocaveris, quid consilii ceperis, quem nostrum ignorare arbitraris? O tempora, o mores! Senatus haec intellegit. Consul videt; hic tamen vivit. Vivit? immo vero etiam in senatum venit, fit publici consilii particeps, notat et designat oculis ad caedem unum quemque nostrum. Nos autem fortes viri satis facere rei publicae videmur, si istius furorem ac tela vitemus. Ad mortem te, Catilina, duci iussu consulis iam pridem oportebat, in te conferri pestem, quam tu in nos machinaris. An vero vir amplissumus, P. Scipio, pontifex maximus, Ti. Gracchum mediocriter labefactantem statum rei publicae privatus interfecit; Catilinam orbem terrae caede atque incendiis vastare cupientem nos consules perferemus? Nam illa nimis antiqua praetereo, quod C. Servilius Ahala Sp. Maelium novis rebus studentem manu sua occidit. Fuit, fuit ista quondam in hac re publica virtus, ut viri fortes acrioribus suppliciis civem perniciosum quam acerbissimum hostem coercerent. Habemus senatus consultum in te, Catilina, vehemens et grave, non deest rei publicae consilium neque auctoritas huius ordinis; nos, nos, dico aperte, consules desumus.",

            new[] 
            {
                "Quo usque tandem abutere, Catilina, patientia nostra? quam diu etiam furor iste tuus nos eludet? quem ad finem sese effrenata iactabit audacia? Nihilne te nocturnum praesidium Palati, nihil urbis vigiliae, nihil timor populi, nihil concursus bonorum omnium, nihil hic munitissimus habendi senatus locus, nihil horum ora voltusque moverunt? Patere tua consilia non sentis, constrictam iam horum omnium scientia teneri coniurationem tuam non vides? Quid proxima, quid superiore nocte egeris, ubi fueris,", 
                "quos convocaveris, quid consilii ceperis, quem nostrum ignorare arbitraris? O tempora, o mores! Senatus haec intellegit. Consul videt; hic tamen vivit. Vivit? immo vero etiam in senatum venit, fit publici consilii particeps, notat et designat oculis ad caedem unum quemque nostrum. Nos autem fortes viri satis facere rei publicae videmur, si istius furorem ac tela vitemus. Ad mortem te, Catilina, duci iussu consulis iam pridem oportebat, in te conferri pestem, quam tu in nos machinaris. An vero vir amplissumus,", 
                "P. Scipio, pontifex maximus, Ti. Gracchum mediocriter labefactantem statum rei publicae privatus interfecit; Catilinam orbem terrae caede atque incendiis vastare cupientem nos consules perferemus? Nam illa nimis antiqua praetereo, quod C. Servilius Ahala Sp. Maelium novis rebus studentem manu sua occidit. Fuit, fuit ista quondam in hac re publica virtus, ut viri fortes acrioribus suppliciis civem perniciosum quam acerbissimum hostem coercerent. Habemus senatus consultum in te, Catilina, vehemens et grave,", 
                "non deest rei publicae consilium neque auctoritas huius ordinis; nos, nos, dico aperte, consules desumus."
            }
        ),
    ];
}
