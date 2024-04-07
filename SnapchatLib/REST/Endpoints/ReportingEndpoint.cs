using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using SnapchatLib.Exceptions;
using SnapchatLib.Extras;
using SnapProto.Impala.Stories;
using SnapProto.Snapchat.Abuse.Support;
using static SnapProto.Snapchat.Abuse.Support.SCReportReport.Types;

namespace SnapchatLib.Exceptions
{
    public class NoStoriesFoundException : Exception
    {
        public NoStoriesFoundException() : base("No stories to report")
        {
        }
    }
}

namespace SnapchatLib.REST.Endpoints
{
    public interface IReportingEndpoint
    {
        Task ReportUserRandom(string username);
        Task ReportBusinessStoryRandom(string username);
    }

    internal class ReportingEndpoint : EndpointAccessor, IReportingEndpoint
    {


        internal static readonly EndpointInfo EndpointInfo = new()
        {
            Url = "/rpc/getBusinessStoryManifest",
            BaseEndpoint = RequestConfigurator.ProStoriesEndpoint,
            Requirements = EndpointRequirements.XSnapAccessToken | EndpointRequirements.XSnapchatUUID | EndpointRequirements.OSUserAgent | EndpointRequirements.AcceptProtoBuf | EndpointRequirements.UseBusinessAccessToken
        };

        internal static readonly EndpointInfo EndpointInfo2 = new() { Url = "/snapchat.abuse.support.ReportService/SendReport", Requirements = EndpointRequirements.XRequestUUID };
        public ReportingEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
        {
        }
        private readonly HashSet<string> Comments = new HashSet<string>()
        {
            // English
            "I'm really upset by this user's offensive and hateful posts. It's deeply disturbing to me.",
            "I'm concerned about a user who's cyberbullying and harassing others. It feels so unfair and hurtful.",
            "I'm alarmed by an account spreading false info and inciting violence. This is dangerous and needs urgent attention.",
            "I can't tolerate a user using derogatory language and targeting individuals. It's completely unacceptable and hurtful.",
            "I'm worried that this account might be involved in identity theft. This is a serious concern and feels really unsafe.",
            "I need to report a user for sharing explicit content. It's totally inappropriate and unsettling.",
            "I'm distressed by a user promoting self-harm. This is alarming and needs immediate intervention.",
            "I came across an account doxing people. It's a gross violation of privacy and really concerning.",
            "I'm annoyed and worried about an account that might be spamming and phishing. It's potentially harmful.",
            "I'm deeply disturbed by hate speech and discrimination from a user. It's utterly unacceptable and hurtful.",
            "I'm concerned about an account involved in online scams. It feels unsafe for our community.",
            "I received a suspicious message with links. I'm worried it's malware or a virus.",
            "I found an account promoting illegal activities. This is clearly against the guidelines and concerning.",
            "There's a user who seems to be stalking others. It's creepy and invasive.",
            "I'm alarmed that an account might be involved in hacking. It's a serious security breach.",
            "I'm upset by a user sharing non-consensual intimate media. It's disturbing and illegal.",
            "I'm troubled by targeted harassment and stalking from a user. It's scary and needs to be stopped.",
            "I'm concerned about an account spreading harmful conspiracy theories. It's misleading and dangerous.",
            "I suspect a user is using multiple accounts for harassment. It's intimidating and wrong.",
            "I'm disturbed by content promoting self-harm. It's deeply upsetting and harmful.",
            "I'm upset about non-consensual sharing of intimate images. It's a violation of privacy and deeply wrong.",
            "I found an account promoting illegal activities. This is dangerous and illegal.",
            "I received threats from a user. It's abusive and frightening.",
            "I'm upset by hate speech from a user. It's deeply offensive and wrong.",
            "I'm concerned about health misinformation being spread. It's dangerous and irresponsible.",
            "I'm worried about online grooming from a user. It's predatory and really concerning.",
            "A user is sharing graphic content without warnings. It's shocking and inappropriate.",
            "I'm alarmed by an account inciting political unrest. It's propagandistic and destabilizing.",
            "I'm distressed by continuous harassment and defamation from a user. It's damaging and hurtful.",
            "I found an account impersonating a public figure. It's misleading and wrong.",
            "I suspect fake account activity to manipulate engagement. It's deceptive and unfair.",
            "I'm upset about doxxing and privacy invasion. It's a serious violation and really concerning.",
    
            // Spanish
            "Estoy realmente molesto por las publicaciones ofensivas y de odio de este usuario. Es profundamente perturbador para mí.",
            "Me preocupa un usuario que está acosando cibernéticamente y hostigando a otros. Se siente tan injusto y doloroso.",
            "Estoy alarmado por una cuenta que difunde información falsa e incita a la violencia. Esto es peligroso y necesita atención urgente.",
            "No puedo tolerar a un usuario que usa lenguaje denigrante y se enfoca en individuos. Es completamente inaceptable y doloroso.",
            "Me preocupa que esta cuenta pueda estar involucrada en robo de identidad. Es una preocupación seria y se siente realmente inseguro.",
            "Necesito reportar a un usuario por compartir contenido explícito. Es totalmente inapropiado y perturbador.",
            "Estoy angustiado por un usuario que promueve el autolesionarse. Esto es alarmante y necesita intervención inmediata.",
            "Me encontré con una cuenta que hace doxing a personas. Es una grave violación de la privacidad y realmente preocupante.",
            "Estoy molesto y preocupado por una cuenta que podría estar enviando spam y haciendo phishing. Es potencialmente dañino.",
            "Estoy profundamente perturbado por el discurso de odio y la discriminación de un usuario. Es totalmente inaceptable y doloroso.",
            "Estoy preocupado por una cuenta involucrada en estafas en línea. Se siente inseguro para nuestra comunidad.",
            "Recibí un mensaje sospechoso con enlaces. Me preocupa que sea malware o un virus.",
            "Encontré una cuenta que promueve actividades ilegales. Esto está claramente en contra de las directrices y es preocupante.",
            "Hay un usuario que parece estar acosando a otros. Es espeluznante e invasivo.",
            "Estoy alarmado de que una cuenta pueda estar involucrada en hacking. Es una grave violación de la seguridad.",
            "Estoy molesto por un usuario que comparte medios íntimos no consensuales. Es perturbador e ilegal.",
            "Estoy preocupado por el acoso y acecho dirigido de un usuario. Es aterrador y necesita ser detenido.",
            "Me preocupa una cuenta que difunde teorías de conspiración dañinas. Es engañoso y peligroso.",
            "Sospecho que un usuario está usando varias cuentas para acoso. Es intimidante y incorrecto.",
            "Estoy perturbado por contenido que promueve autolesionarse. Es profundamente molesto y dañino.",
            "Estoy molesto por la compartición no consensual de imágenes íntimas. Es una violación de la privacidad y profundamente incorrecto.",
            "Encontré una cuenta que promueve actividades ilegales. Esto es peligroso e ilegal.",
            "Recibí amenazas de un usuario. Es abusivo y aterrador.",
            "Estoy molesto por el discurso de odio de un usuario. Es profundamente ofensivo e incorrecto.",
            "Me preocupa la desinformación de salud que se está difundiendo. Es peligroso e irresponsable.",
            "Me preocupa el acoso en línea de un usuario. Es depredador y realmente preocupante.",
            "Un usuario está compartiendo contenido gráfico sin advertencias. Es impactante e inapropiado.",
            "Estoy alarmado por una cuenta que incita a la inestabilidad política. Es propagandístico y desestabilizador.",
            "Estoy angustiado por el acoso continuo y la difamación de un usuario. Es dañino y doloroso.",
            "Encontré una cuenta que se hace pasar por una figura pública. Es engañoso e incorrecto.",
            "Sospecho actividad de cuenta falsa para manipular el compromiso. Es engañoso e injusto.",
            "Estoy molesto por el doxing y la invasión de la privacidad. Es una grave violación y realmente preocupante.",
    
            // French
            "Estoy realmente molesto por las publicaciones ofensivas y de odio de este usuario. Es profundamente perturbador para mí.",
            "Me preocupa un usuario que está acosando cibernéticamente y hostigando a otros. Se siente tan injusto y doloroso.",
            "Estoy alarmado por una cuenta que difunde información falsa e incita a la violencia. Esto es peligroso y necesita atención urgente.",
            "No puedo tolerar a un usuario que usa lenguaje denigrante y se enfoca en individuos. Es completamente inaceptable y doloroso.",
            "Me preocupa que esta cuenta pueda estar involucrada en robo de identidad. Es una preocupación seria y se siente realmente inseguro.",
            "Necesito reportar a un usuario por compartir contenido explícito. Es totalmente inapropiado y perturbador.",
            "Estoy angustiado por un usuario que promueve el autolesionarse. Esto es alarmante y necesita intervención inmediata.",
            "Me encontré con una cuenta que hace doxing a personas. Es una grave violación de la privacidad y realmente preocupante.",
            "Estoy molesto y preocupado por una cuenta que podría estar enviando spam y haciendo phishing. Es potencialmente dañino.",
            "Estoy profundamente perturbado por el discurso de odio y la discriminación de un usuario. Es totalmente inaceptable y doloroso.",
            "Estoy preocupado por una cuenta involucrada en estafas en línea. Se siente inseguro para nuestra comunidad.",
            "Recibí un mensaje sospechoso con enlaces. Me preocupa que sea malware o un virus.",
            "Encontré una cuenta que promueve actividades ilegales. Esto está claramente en contra de las directrices y es preocupante.",
            "Hay un usuario que parece estar acosando a otros. Es espeluznante e invasivo.",
            "Estoy alarmado de que una cuenta pueda estar involucrada en hacking. Es una grave violación de la seguridad.",
            "Estoy molesto por un usuario que comparte medios íntimos no consensuales. Es perturbador e ilegal.",
            "Estoy preocupado por el acoso y acecho dirigido de un usuario. Es aterrador y necesita ser detenido.",
            "Me preocupa una cuenta que difunde teorías de conspiración dañinas. Es engañoso y peligroso.",
            "Sospecho que un usuario está usando varias cuentas para acoso. Es intimidante y incorrecto.",
            "Estoy perturbado por contenido que promueve autolesionarse. Es profundamente molesto y dañino.",
            "Estoy molesto por la compartición no consensual de imágenes íntimas. Es una violación de la privacidad y profundamente incorrecto.",
            "Encontré una cuenta que promueve actividades ilegales. Esto es peligroso e ilegal.",
            "Recibí amenazas de un usuario. Es abusivo y aterrador.",
            "Estoy molesto por el discurso de odio de un usuario. Es profundamente ofensivo e incorrecto.",
            "Me preocupa la desinformación de salud que se está difundiendo. Es peligroso e irresponsable.",
            "Me preocupa el acoso en línea de un usuario. Es depredador y realmente preocupante.",
            "Un usuario está compartiendo contenido gráfico sin advertencias. Es impactante e inapropiado.",
            "Estoy alarmado por una cuenta que incita a la inestabilidad política. Es propagandístico y desestabilizador.",
            "Estoy angustiado por el acoso continuo y la difamación de un usuario. Es dañino y doloroso.",
            "Encontré una cuenta que se hace pasar por una figura pública. Es engañoso e incorrecto.",
            "Sospecho actividad de cuenta falsa para manipular el compromiso. Es engañoso e injusto.",
            "Estoy molesto por el doxing y la invasión de la privacidad. Es una grave violación y realmente preocupante.",
    
             // German
            "Ich bin wirklich aufgebracht über die beleidigenden und hasserfüllten Beiträge dieses Benutzers. Es ist zutiefst beunruhigend für mich.",
            "Ich mache mir Sorgen über einen Benutzer, der Cybermobbing betreibt und andere belästigt. Es fühlt sich so unfair und verletzend an.",
            "Ich bin alarmiert über ein Konto, das falsche Informationen verbreitet und Gewalt anstiftet. Das ist gefährlich und benötigt dringende Aufmerksamkeit.",
            "Ich kann es nicht tolerieren, dass ein Benutzer abwertende Sprache verwendet und Personen gezielt angreift. Das ist völlig inakzeptabel und verletzend.",
            "Ich mache mir Sorgen, dass dieses Konto in Identitätsdiebstahl verwickelt sein könnte. Das ist eine ernste Sorge und fühlt sich wirklich unsicher an.",
            "Ich muss einen Benutzer melden, der explizite Inhalte teilt. Das ist völlig unangebracht und beunruhigend.",
            "Ich bin bestürzt über einen Benutzer, der Selbstverletzung fördert. Das ist alarmierend und benötigt sofortige Intervention.",
            "Ich bin auf ein Konto gestoßen, das Doxing betreibt. Das ist ein grober Verstoß gegen die Privatsphäre und wirklich besorgniserregend.",
            "Ich bin genervt und besorgt über ein Konto, das möglicherweise Spam und Phishing betreibt. Das ist potenziell schädlich.",
            "Ich bin zutiefst beunruhigt über Hassrede und Diskriminierung von einem Benutzer. Das ist absolut inakzeptabel und verletzend.",
            "Ich mache mir Sorgen über ein Konto, das in Online-Betrug verwickelt ist. Das fühlt sich unsicher für unsere Gemeinschaft an.",
            "Ich habe eine verdächtige Nachricht mit Links erhalten. Ich mache mir Sorgen, dass es Malware oder ein Virus sein könnte.",
            "Ich habe ein Konto gefunden, das illegale Aktivitäten fördert. Das steht eindeutig gegen die Richtlinien und ist besorgniserregend.",
            "Es gibt einen Benutzer, der anscheinend andere stalkt. Das ist gruselig und invasiv.",
            "Ich bin alarmiert, dass ein Konto in Hacking verwickelt sein könnte. Das ist ein ernsthafter Sicherheitsverstoß.",
            "Ich bin aufgebracht über einen Benutzer, der nicht-einvernehmliche intime Medien teilt. Das ist verstörend und illegal.",
            "Ich bin beunruhigt über gezieltes Mobbing und Stalking von einem Benutzer. Das ist beängstigend und muss gestoppt werden.",
            "Ich mache mir Sorgen über ein Konto, das schädliche Verschwörungstheorien verbreitet. Das ist irreführend und gefährlich.",
            "Ich vermute, dass ein Benutzer mehrere Konten für Belästigungen verwendet. Das ist einschüchternd und falsch.",
            "Ich bin beunruhigt über Inhalte, die Selbstverletzung fördern. Das ist zutiefst aufwühlend und schädlich.",
            "Ich bin aufgebracht über die nicht-einvernehmliche Weitergabe von intimen Bildern. Das ist ein Verstoß gegen die Privatsphäre und zutiefst falsch.",
            "Ich habe ein Konto gefunden, das illegale Aktivitäten fördert. Das ist gefährlich und illegal.",
            "Ich habe Drohungen von einem Benutzer erhalten. Das ist missbräuchlich und erschreckend.",
            "Ich bin aufgebracht über Hassrede von einem Benutzer. Das ist zutiefst beleidigend und falsch.",
            "Ich mache mir Sorgen über die Verbreitung von gefährlichen Gesundheits-Fehlinformationen. Das ist gefährlich und verantwortungslos.",
            "Ich mache mir Sorgen über Online-Grooming von einem Benutzer. Das ist räuberisch und wirklich besorgniserregend.",
            "Ein Benutzer teilt grafische Inhalte ohne Warnungen. Das ist schockierend und unangemessen.",
            "Ich bin alarmiert über ein Konto, das politische Unruhen anstachelt. Das ist propagandistisch und destabilisierend.",
            "Ich bin verstört über kontinuierliches Mobbing und Verleumdung von einem Benutzer. Das ist schädlich und verletzend.",
            "Ich habe ein Konto gefunden, das sich als eine öffentliche Figur ausgibt. Das ist irreführend und falsch.",
            "Ich vermute gefälschte Kontoaktivität zur Manipulation von Engagement. Das ist täuschend und unfair.",
            "Ich bin aufgebracht über Doxing und Verletzung der Privatsphäre. Das ist ein schwerer Verstoß und wirklich besorgniserregend.",
    
            // Arabic
            "أنا منزعج حقًا من المنشورات المسيئة والكراهية لهذا المستخدم. إنها مقلقة للغاية بالنسبة لي.",
            "أنا قلق بشأن مستخدم يمارس التنمر الإلكتروني ويتحرش بالآخرين. يبدو ذلك غير عادل ومؤلم.",
            "أنا منذر بحساب ينشر معلومات خاطئة ويحرض على العنف. هذا خطير ويحتاج إلى اهتمام عاجل.",
            "لا أستطيع تحمل مستخدم يستخدم لغة مهينة ويستهدف أفراد. هذا غير مقبول على الإطلاق ومؤذٍ.",
            "أنا قلق من أن هذا الحساب قد يكون متورطًا في سرقة الهوية. هذا أمر خطير ويشعرني بعدم الأمان.",
            "أحتاج إلى الإبلاغ عن مستخدم يشارك محتوى صريح. هذا غير لائق تمامًا ومقلق.",
            "أنا مضطرب بسبب مستخدم يروج للإيذاء الذاتي. هذا مقلق ويحتاج إلى تدخل فوري.",
            "صادفت حسابًا يقوم بفضح معلومات الأشخاص. هذا انتهاك خطير للخصوصية ومقلق للغاية.",
            "أنا مزعج وقلق بشأن حساب قد يكون يرسل رسائل غير مرغوب فيها ويقوم بالتصيد الاحتيالي. هذا محتمل الضرر.",
            "أنا مضطرب بعمق من خطاب الكراهية والتمييز من مستخدم. هذا غير مقبول على الإطلاق ومؤلم.",
            "أنا قلق بشأن حساب متورط في الاحتيال عبر الإنترنت. يبدو غير آمن لمجتمعنا.",
            "تلقيت رسالة مشبوهة بروابط. أنا قلق من أنها قد تكون برامج ضارة أو فيروس.",
            "وجدت حسابًا يروج للأنشطة غير القانونية. هذا يتعارض بوضوح مع الإرشادات ومقلق.",
            "هناك مستخدم يبدو أنه يتعقب الآخرين. هذا مخيف وتدخلي.",
            "أنا منذر بأن حسابًا قد يكون متورطًا في القرصنة. هذا خرق أمني خطير.",
            "أنا منزعج من مستخدم يشارك وسائط حميمة غير متوافقة. هذا مقلق وغير قانوني.",
            "أنا متضايق من التحرش والمطاردة المستهدفة من مستخدم. هذا مخيف ويحتاج إلى التوقف.",
            "أنا قلق بشأن حساب ينشر نظريات مؤامرة ضارة. هذا مضلل وخطير.",
            "أشك في أن مستخدمًا يستخدم حسابات متعددة للتحرش. هذا مخيف وخاطئ.",
            "أنا مضطرب بسبب المحتوى الذي يروج للإيذاء الذاتي. هذا مزعج للغاية وضار.",
            "أنا منزعج من مشاركة صور حميمة غير متوافقة. هذا انتهاك للخصوصية وخاطئ للغاية.",
            "وجدت حسابًا يروج للأنشطة غير القانونية. هذا خطير وغير قانوني.",
            "تلقيت تهديدات من مستخدم. هذا مسيء ومخيف.",
            "أنا منزعج من خطاب الكراهية من مستخدم. هذا مسيء للغاية وخاطئ.",
            "أنا قلق بشأن انتشار معلومات صحية خاطئة. هذا خطير وغير مسؤول.",
            "أنا قلق بشأن تحرش مستخدم عبر الإنترنت. هذا استغلالي ومقلق للغاية.",
            "مستخدم يشارك محتوى جرافيكي بدون تحذيرات. هذا صادم وغير لائق.",
            "أنا منذر بحساب يحرض على الاضطرابات السياسية. هذا دعائي ومزعزع للاستقرار.",
            "أنا مضطرب من التحرش المستمر والتشهير من مستخدم. هذا ضار ومؤلم.",
            "وجدت حسابًا ينتحل شخصية شخصية عامة. هذا مضلل وخاطئ.",
            "أشك في نشاط حساب مزيف للتلاعب بالتفاعل. هذا خادع وغير عادل.",
            "أنا منزعج من التعرض للخصوصية وغزو الخصوصية. هذا انتهاك خطير ومقلق للغاية.",

            //Dutch
            "Ik ben echt van streek door de beledigende en haatdragende berichten van deze gebruiker. Het is diep verontrustend voor mij.",
            "Ik maak me zorgen over een gebruiker die cyberpesten en anderen lastigvalt. Het voelt zo oneerlijk en pijnlijk.",
            "Ik ben gealarmeerd door een account dat valse informatie verspreidt en geweld aanwakkert. Dit is gevaarlijk en vereist dringende aandacht.",
            "Ik kan niet tolereren dat een gebruiker denigrerende taal gebruikt en individuen target. Het is volkomen onacceptabel en kwetsend.",
            "Ik ben bezorgd dat dit account betrokken kan zijn bij identiteitsdiefstal. Dit is een serieuze zorg en voelt echt onveilig.",
            "Ik moet een gebruiker rapporteren voor het delen van expliciete inhoud. Het is totaal ongepast en verontrustend.",
            "Ik ben ontdaan door een gebruiker die zelfbeschadiging promoot. Dit is alarmerend en vereist onmiddellijke interventie.",
            "Ik kwam een account tegen dat doxing van mensen doet. Het is een grove schending van de privacy en echt zorgwekkend.",
            "Ik ben geïrriteerd en bezorgd over een account dat mogelijk spamt en phisht. Het is potentieel schadelijk.",
            "Ik ben diep verstoord door haatspraak en discriminatie van een gebruiker. Het is volkomen onaanvaardbaar en pijnlijk.",
            "Ik ben bezorgd over een account dat betrokken is bij online oplichting. Het voelt onveilig voor onze gemeenschap.",
            "Ik ontving een verdacht bericht met links. Ik maak me zorgen dat het malware of een virus is.",
            "Ik vond een account dat illegale activiteiten promoot. Dit is duidelijk tegen de richtlijnen en zorgwekkend.",
            "Er is een gebruiker die anderen lijkt te stalken. Het is griezelig en invasief.",
            "Ik ben gealarmeerd dat een account betrokken kan zijn bij hacken. Dit is een ernstige inbreuk op de beveiliging.",
            "Ik ben van streek door een gebruiker die intieme media zonder toestemming deelt. Het is verontrustend en illegaal.",
            "Ik ben verontrust door gerichte intimidatie en stalking van een gebruiker. Het is eng en moet worden gestopt.",
            "Ik maak me zorgen over een account dat schadelijke samenzweringstheorieën verspreidt. Het is misleidend en gevaarlijk.",
            "Ik vermoed dat een gebruiker meerdere accounts gebruikt voor intimidatie. Het is intimiderend en verkeerd.",
            "Ik ben verstoord door inhoud die zelfbeschadiging promoot. Het is diep verontrustend en schadelijk.",
            "Ik ben van streek over het niet-consensuele delen van intieme afbeeldingen. Het is een schending van de privacy en diep verkeerd.",
            "Ik vond een account dat illegale activiteiten promoot. Dit is gevaarlijk en illegaal.",
            "Ik heb bedreigingen van een gebruiker ontvangen. Het is beledigend en beangstigend.",
            "Ik ben van streek door haatspraak van een gebruiker. Het is diep beledigend en verkeerd.",
            "Ik ben bezorgd over het verspreiden van onjuiste gezondheidsinformatie. Het is gevaarlijk en onverantwoordelijk.",
            "Ik maak me zorgen over online grooming door een gebruiker. Het is roofzuchtig en echt zorgwekkend.",
            "Een gebruiker deelt grafische inhoud zonder waarschuwingen. Het is schokkend en ongepast.",
            "Ik ben gealarmeerd door een account dat politieke onrust aanwakkert. Het is propagandistisch en destabiliserend.",
            "Ik ben van streek door aanhoudende intimidatie en laster van een gebruiker. Het is schadelijk en pijnlijk.",
            "Ik vond een account dat zich voordoet als een publiek figuur. Het is misleidend en verkeerd.",
            "Ik vermoed nepaccountactiviteit om betrokkenheid te manipuleren. Het is misleidend en oneerlijk.",
            "Ik ben van streek over doxing en inbreuk op privacy. Het is een ernstige schending en echt zorgwekkend."
        };

        public async Task ReportUser(string username, string Comment, SCReportReportReason Reason)
        {
            var friendUserId = await SnapchatClient.FindUserFromCache(username);
            if (string.IsNullOrWhiteSpace(friendUserId)) throw new UsernameNotFoundException(username);

            var request = new SCReportSendReportRequest
            {
                Report = new SCReportReport
                {
                    Reported = new SCReportReported
                    {
                        UserReportData = new SCReportUserReportData
                        {
                            ReportedUserId = friendUserId,
                            IsUserContentsReport = true
                        }
                    },
                    Comment = Comment,
                    Reason = Reason,
                    Entrypoint = new SCReportReportEntryPoint 
                    {
                        Feature = "PROFILE",
                        Subfeature = "NONE"
                    }
                }
            };

            await SnapchatGrpcClient.ReportUserAsync(request);
        }

        public async Task ReportUserRandom(string username)
        {
            SCReportReportReason reason;

            var random = new Random();
            var reportReasonValues = Enum.GetValues(typeof(SCReportReportReason))
                                         .Cast<SCReportReportReason>()
                                         .Where(r => r != SCReportReportReason.ReasonUnset)
                                         .ToList();

            reason = reportReasonValues[random.Next(reportReasonValues.Count)];

            int randomIndex = random.Next(Comments.Count);
            await ReportUser(username, Comments.ElementAt(randomIndex), reason);
        }

        public async Task ReportBusinessStoryRandom(string username)
        {
            SCReportReportReason reason;

            var random = new Random();
            var reportReasonValues = Enum.GetValues(typeof(SCReportReportReason))
                                         .Cast<SCReportReportReason>()
                                         .Where(r => r != SCReportReportReason.ReasonUnset)
                                         .ToList();

            reason = reportReasonValues[random.Next(reportReasonValues.Count)];

            await ReportBusinessStory(username, reason);
        }
        public async Task ReportBusinessStory(string username, SCReportReportReason Reason)
        {
            try
            {
                var finduser = await SnapchatClient.FindUsersViaSearch(username);

                string ownerId = "";

                foreach (var section in finduser.SectionsArray)
                {
                    foreach (var result in section.ResultsArray)
                    {
                        if (result.User != null)
                        {
                            var user = result.User;

                            if (user.Username == username || user.MutableUsername == username)
                            {
                                ownerId = user.SnapProId;
                                break;
                            }
                        }

                        if (result.SnapProEntity != null)
                        {
                            var snapProEntity = result.SnapProEntity;

                            if (snapProEntity.Profile?.BusinessProfile?.HostAccountUsername == username || snapProEntity.Profile?.BusinessProfile?.HostAccountMutableUsername == username)
                            {
                                ownerId = snapProEntity.Profile.BusinessProfile.IdP;
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(ownerId))
                {
                    // Send data via HTTP
                    var getBusinessStoryManifestRequest = new IMPGetBusinessStoryManifestRequest
                    {
                        BusinessId = ownerId
                    };
                    using var byteArrayContent = new ByteArrayContent(getBusinessStoryManifestRequest.ToByteArray());
                    byteArrayContent.Headers.Add("Content-Type", "application/x-protobuf");
                    var response = await Send(EndpointInfo, byteArrayContent);
                    var result = IMPGetBusinessStoryManifestResponse.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());

                    foreach (var story in result.Manifest.ElementsArray)
                    {
                        var request = new SCReportSendReportRequest
                        {
                            Report = new SCReportReport
                            {
                                Reported = new SCReportReported
                                {
                                    PublicUserStorySnapReportData = new SCReportPublicUserStorySnapReportData
                                    {
                                        SnapId = story.IdP
                                    }
                                },
                                Reason = Reason
                            }
                        };

                        await SnapchatGrpcClient.ReportUserAsync(request);
                    }
                }
                else
                {
                    throw new Exception("Owner ID not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}