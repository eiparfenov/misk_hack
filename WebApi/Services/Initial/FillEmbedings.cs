using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Services.Initial;

public class FillEmbedings(IServiceScopeFactory serviceScopeFactory): BackgroundService
{
    public const int CurrentEmbedingsVersion = 2;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync(stoppingToken);
        var questionsExists = await db.Questions.AnyAsync(stoppingToken);
        if (!questionsExists)
        {
          db.Categories.AddRange(JsonSerializer.Deserialize<Category[]>(Json)!);
          await db.SaveChangesAsync(stoppingToken);
        }
        var embedingService = scope.ServiceProvider.GetRequiredService<IEmbedingsService>();
        while (!stoppingToken.IsCancellationRequested)
        {
            var existedEmbedings = await db.Embedings
                .Where(e => e.Version == CurrentEmbedingsVersion)
                .Select(e => e.QuestionId)
                .ToArrayAsync(stoppingToken);
            var questionsToFill = await db.Questions
                .Where(q => !existedEmbedings.Contains(q.Id))
                .Select(q => new { q.Id, q.Example })
                .ToArrayAsync(stoppingToken);
            if (questionsToFill.Length == 0)
            {
              break;
            }
            List<Embeding> embeds = [];
            foreach (var q in questionsToFill)
            {
                var embed = await embedingService.GetEmbedings(q.Example);
                if (embed is null)
                {
                    Console.WriteLine($"No embeds for {q.Example}");
                    continue;
                }

                Console.WriteLine("succesfully loaded embed");
                embeds.Add(new Embeding()
                {
                    Id = Guid.NewGuid(),
                    QuestionId = q.Id,
                    Version = CurrentEmbedingsVersion,
                    Vector = new Pgvector.Vector(embed)
                });
            }
            db.AddRange(embeds);
            await db.SaveChangesAsync(stoppingToken);
        }
    }

    public const string Json = """
                               [
                                 {
                                   "Id": "9e848ad9-f3d9-419b-90d5-4506572d68b2",
                                   "Title": "Новые клиенты",
                                   "SubCategories": [
                                     {
                                       "Id": "7b628904-d0a1-47cf-bbf3-e43d08979a87",
                                       "Title": "Регистрация и онбординг",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "461f892d-d150-42f4-9059-42dcad63f865",
                                           "Example": "Как стать клиентом банка онлайн?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Стать клиентом ВТБ (Беларусь) можно онлайн через сайт vtb.by или мобильное приложение VTB mBank. Для регистрации потребуется паспорт и номер телефона. После регистрации через МСИ (Межбанковскую систему идентификации) вы получите доступ к банковским услугам.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "29ea4ed1-3617-4c7b-a9d5-6c4b8cec2fed",
                                           "Example": "Регистрация через МСИ (Межбанковская система идентификации)",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "МСИ позволяет пройти идентификацию онлайн, используя данные других банков, где вы уже являетесь клиентом. Это упрощает процедуру регистрации и делает её быстрой и безопасной.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4d4f2532-155e-4ba3-bacd-519fc47f76a4",
                                           "Example": "Документы для регистрации нового клиента",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для регистрации в качестве нового клиента необходим паспорт гражданина Республики Беларусь и контактный номер мобильного телефона для получения SMS-подтверждений.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "b06aacec-3387-4286-ab1f-e2a2e531f518",
                                       "Title": "Первые шаги",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "fd893e96-349e-435e-858f-eaf7635c2650",
                                           "Example": "Первый вход в Интернет-банк",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "После регистрации вы получите логин и пароль для входа в систему Интернет-банк. При первом входе рекомендуется изменить временный пароль на постоянный и настроить дополнительные параметры безопасности.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "0886209e-fdf6-48d0-bd35-4dfb2a588516",
                                           "Example": "Как скачать и настроить мобильное приложение?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Мобильное приложение VTB mBank можно скачать в App Store для iOS или Google Play для Android. После установки войдите с логином и паролем от Интернет-банка и пройдите первоначальную настройку.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     }
                                   ]
                                 },
                                 {
                                   "Id": "b43e6fe8-fe84-43b4-b34f-0116bd297c06",
                                   "Title": "Техническая поддержка",
                                   "SubCategories": [
                                     {
                                       "Id": "310e15a4-290f-4dc0-b581-eb594cd6625b",
                                       "Title": "Проблемы и решения",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "22bfd281-401c-4fb7-abc1-b50a9172b546",
                                           "Example": "Не могу войти в Интернет-банк",
                                           "Priority": "высокий",
                                           "Audience": "все клиенты",
                                           "Answer": "Если не получается войти в Интернет-банк, проверьте правильность ввода логина и пароля. При забытом пароле воспользуйтесь функцией восстановления. Если проблема не решается, обратитесь в контакт-центр по номеру 250 или +375 (17/29/33) 309 15 15.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "7eb7dbba-274a-4ea3-b989-f53a6a2bbbd1",
                                           "Example": "Я нахожусь за пределами Республики Беларусь. Как я могу связаться с работниками банка, за исключением телефонного звонка?",
                                           "Priority": "высокий",
                                           "Audience": "все клиенты",
                                           "Answer": "Вы можете получить онлайн-консультацию (в текстовом формате) в будние дни с 9:00 до 17:30, написав специалисту банка в Telegram либо в чат на сайте (ссылки на сайте банка).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "daa42bde-5d42-4e50-8c44-77d0f6ddd342",
                                           "Example": "Забыл пароль от мобильного приложения",
                                           "Priority": "высокий",
                                           "Audience": "все клиенты",
                                           "Answer": "Если Вы забыли логин или пароль доступа, доступна процедура восстановления доступа к системе ДБО со стартового экрана. Для этого требуется нажать на кнопку «Забыли логин или пароль?».",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "463b16d7-ca39-4263-a1e5-255a6df7eea8",
                                           "Example": "Карта заблокирована - что делать?",
                                           "Priority": "высокий",
                                           "Audience": "все клиенты",
                                           "Answer": "Если карта заблокирована, обратитесь в контакт-центр банка по телефону 250 для выяснения причины блокировки. В большинстве случаев карту можно разблокировать по телефону после прохождения идентификации.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "c355a9e4-9775-4585-b898-3b4411e12435",
                                           "Example": "Могу ли я оставить обращение, не обращаясь лично в офис банка?\n",
                                           "Priority": "высокий",
                                           "Audience": "все клиенты",
                                           "Answer": "Вы можете воспользоваться формой \"Обратная связь\"(https://www.vtb.by/elektronnoe-pismo) на главной странице сайта банка ВТБ. Ваше обращение будет рассмотрено руководством банка в соответствии с Законом Республики Беларусь \"Об обращениях граждан и юридических лиц\" №300-З.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2ebf1706-52d4-4238-97aa-a84dc32f7730",
                                           "Example": "Перевел денежные средства на карту, но на балансе их нет.",
                                           "Priority": "высокий",
                                           "Audience": "все клиенты",
                                           "Answer": "В зависимости от условий Вашей карточки и способа совершения операции по карточке/ счету, погашение задолженности по договору и доступность денежных средств с использованием карточки будет обеспечена в следующее время: https://www.vtb.by/sites/default/files/sites/default/files/anketa/o_vremeni_zachisleniya_na_bpk_s_23.05.2021.pdf",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     }
                                   ]
                                 },
                                 {
                                   "Id": "5224cb93-2bb6-4fbc-bd6c-30bb5f0d6885",
                                   "Title": "Продукты - Карты",
                                   "SubCategories": [
                                     {
                                       "Id": "e46e10ee-2f98-4164-b297-a15b326cfb64",
                                       "Title": "Дебетовые карты - MORE",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "01bbb2cd-2c62-4341-91d8-d73ac2ccdccf",
                                           "Example": "Как оформить карту MORE?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карту MORE можно оформить в любом отделении банка ВТБ (Беларусь) с паспортом или онлайн через Интернет-банк и мобильное приложение VTB mBank. Карта выдается бесплатно.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "6a597df9-67fb-444a-a714-a1d396be7def",
                                           "Example": "Какие преимущества у карты MORE?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта MORE предоставляет возможности для повседневной жизни: бесконтактные платежи, снятие наличных в банкоматах ВТБ без комиссии, онлайн-покупки, переводы через приложение.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4dfd2f61-e6e6-4ccc-9e1c-adec5038a7c4",
                                           "Example": "Как бесплатно пополнить карточку MORE?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Пополнить карточку MORE можно бесплатно через банкоматы ВТБ с функцией Cash-In, в отделениях банка, через Интернет-банк с других карт ВТБ, переводом с карты на карту.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f15cdd6b-1aa9-4978-b92f-09c59941d024",
                                           "Example": "Какие лимиты по карте MORE?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Лимиты устанавливаются индивидуально при оформлении карты. Стандартные лимиты можно изменить через Интернет-банк или мобильное приложение VTB mBank в разделе 'Управление лимитами'.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a83a9532-ff3a-4f67-af13-a27f8722524a",
                                           "Example": "Можно ли заказать карту MORE онлайн?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, карту MORE можно заказать онлайн через Интернет-банк или мобильное приложение VTB mBank. После оформления заявки карта будет готова к получению в выбранном отделении банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "c0c1cbab-09e9-409c-8ec2-8d5bc8393201",
                                           "Example": "Сколько стоит обслуживание карты MORE?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Выпуск и обслуживание карты MORE бесплатны для физических лиц при соблюдении условий банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8bb346c5-4ca8-4feb-8d5b-2235db1a6759",
                                           "Example": "Где можно использовать карту MORE?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта MORE принимается во всех торговых точках, где есть терминалы для оплаты картами, в банкоматах для снятия наличных, для онлайн-покупок в интернете.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "bd48b56e-aae8-4ea1-a9d7-a9362e7b2da8",
                                           "Example": "Как активировать карту MORE?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Активация происходит автоматически при проведении первой успешной операции с использованием ПИН-кода: оплата в магазине или снятие наличных в банкомате.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "00210aee-04df-460b-b4c5-c95cf68da429",
                                       "Title": "Дебетовые карты - Форсаж",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "19af5374-b420-4686-b675-46cd0c2da2d7",
                                           "Example": "Как получить карту Форсаж?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Форсаж - это карта мгновенного выпуска. Обратитесь в любое отделение ВТБ (Беларусь) с паспортом, и карта будет выдана сразу при обращении.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "30f0be67-92fb-489b-9ab5-a598d8baa85e",
                                           "Example": "Что такое карта мгновенного выпуска?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта мгновенного выпуска - это карта, которая изготавливается и выдается клиенту непосредственно в отделении банка в течение нескольких минут после подачи заявления.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "b61e6f6a-aa9f-4ee6-911d-075384cb049a",
                                           "Example": "За сколько минут выдается карта Форсаж?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Форсаж выдается в течение нескольких минут после оформления заявления в отделении банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "1dfe90dc-280b-46d5-ace8-25cca1edb2da",
                                           "Example": "Какие документы нужны для карты Форсаж?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления карты Форсаж необходим только паспорт гражданина Республики Беларусь.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a901430c-71de-42aa-a2d7-6ac15ce71fd1",
                                           "Example": "Можно ли сразу использовать карту Форсаж?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, карту Форсаж можно использовать сразу после получения и активации первой операцией с ПИН-кодом.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "792dc8d1-f758-4a53-93a5-35fce033c31b",
                                           "Example": "Какие лимиты у карты Форсаж?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Лимиты по карте «Форсаж» устанавливаются банком; подробные значения суточных и месячных лимитов указаны в условиях выпуска карты и в тарифах банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a9d4057f-f32c-44d4-8076-49deb4c9db65",
                                           "Example": "Сколько действует карта Форсаж?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Срок действия карты Форсаж указывается на самой карте. Обычно карты выдаются на 4 года.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "74fb0584-ddfc-46c7-bd4b-2e8e3944c412",
                                       "Title": "Дебетовые карты - Комплимент",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "d18a6968-421c-45e1-9cf7-f4306e228cd0",
                                           "Example": "Кто может оформить карту Комплимент?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карту Комплимент могут оформить граждане Республики Беларусь от 50 лет. Это специальное предложение для людей старшего возраста.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f3568af5-9e0e-4235-9ca3-405cd5c73c50",
                                           "Example": "Какие льготы предоставляет карта Комплимент?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Комплимент предоставляет льготные условия обслуживания, специальные тарифы и привилегии для клиентов старшего возраста.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ce521ce5-6614-4b4f-ab15-39b755e801e4",
                                           "Example": "Как оформить премиальную карту Комплимент?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления карты Комплимент обратитесь в отделение банка с паспортом. Карта относится к премиальному сегменту Gold.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4d9d38bc-e515-4bd9-849c-7310c354b878",
                                           "Example": "Какой возраст для оформления карты Комплимент?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Комплимент предназначена для клиентов от 50 лет.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "cbd51354-b9d6-405d-8c13-9adbb2e1135d",
                                           "Example": "Сколько стоит карта Комплимент?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Информацию о стоимости обслуживания карты Комплимент уточняйте в тарифах банка или при оформлении в отделении.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "c9756fde-5cd3-4045-8a87-b774636aeae9",
                                           "Example": "Какие услуги включены в карту Комплимент?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Комплимент включает полный комплекс банковских услуг с учетом потребностей клиентов старшего возраста и премиальное обслуживание.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "344c2873-c639-406f-8fd2-e84d0e1b967a",
                                       "Title": "Дебетовые карты - Signature",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "d74c6fb7-3243-4ab0-a5df-c2705eb27b4b",
                                           "Example": "Как получить премиальную карту Signature?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления премиальной карты Signature обратитесь в отделение банка. Карта предназначена для VIP-клиентов с особыми требованиями к сервису.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "5cbd7264-44fb-440f-a6aa-04ac1b3a99e1",
                                           "Example": "Какие привилегии у карты Signature?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Signature предоставляет премиальные привилегии: приоритетное обслуживание, специальные условия, расширенные лимиты, дополнительные сервисы.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a4e49c7e-0677-40a6-95f8-79adcb023ac2",
                                           "Example": "Сколько стоит обслуживание карты Signature?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Тарифы по карте Signature указаны в прейскуранте банка. Уточняйте стоимость при оформлении.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f6787ebc-f0ca-42be-aef0-453f878577cd",
                                           "Example": "Какие требования для оформления Signature?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления карты Signature могут потребоваться подтверждение доходов и соответствие критериям премиального обслуживания.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "84a2b2cd-2d57-450c-bea5-575954d68922",
                                           "Example": "Где принимается карта Signature?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Signature принимается во всем мире везде, где принимаются карты соответствующей платежной системы.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "7f83c87c-8558-4c49-9f73-9a0b7440e9d1",
                                       "Title": "Дебетовые карты - Infinite",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "0efa68cb-9b35-49d5-a867-4996e9386adb",
                                           "Example": "Как оформить карту Infinite?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Infinite - это карта высшего уровня. Для оформления обратитесь к персональному менеджеру в отделении банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a3f73e15-fdf2-423a-a296-bc7599c52524",
                                           "Example": "Что входит в пакет услуг Infinite?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Infinite включает максимальный набор привилегий: консьерж-сервис, страхование, доступ в VIP-залы, индивидуальные условия.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "04efa350-6941-492b-9c2f-82e52eb9dc63",
                                           "Example": "Какие требования к доходам для Infinite?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления карты Infinite установлены высокие требования к доходам клиента. Подробности уточняйте при консультации.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4416dab6-44af-44bd-9194-a19df5a5f69e",
                                           "Example": "Какие привилегии предоставляет Infinite?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Infinite предоставляет все доступные привилегии банка: персональный менеджер, консьерж-сервис, специальные предложения партнеров.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "1ac8285b-da94-435c-b6ab-9f4f090fdb00",
                                       "Title": "Кредитные карты - PLAT/ON",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "8e880c14-91c4-42ee-a750-291e6eaae0ec",
                                           "Example": "Где можно оформить карточку PLAT/ON?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карточку PLAT/ON можно оформить в интернет-банке или М-банкинге, в том числе виртуальную карточку, а также обратившись в любой офис Банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "3263b9f6-fad9-4f7d-82f8-2db7736ffbf6",
                                           "Example": "Какие операции отражаются в бонусный период?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "В бонусный период отражаются безналичные операции покупки товаров и услуг в организации торговли/сервиса, совершенные по карточке PLAT/ON, за исключением операций с MCC, указанных на сайте банка vtb.by.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "032fc3e5-80e2-46e2-82fd-f687fe86e932",
                                           "Example": "Как отражаются операции по бонусным периодам?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Операции по бонусным периодам отражаются по дате отражения по счету. Если бонусный период был изменен после совершения операции, но до ее отражения по счету, операция отразится по вновь установленному бонусному периоду.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "48f3f85b-52c4-4117-a119-b7da5ba052e4",
                                           "Example": "Как переключить бонусный период PLAT/ON?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Переключить бонусный период можно через Интернет-банк или мобильное приложение VTB mBank в разделе управления картой PLAT/ON.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "09307d79-95cd-46a2-91f7-90d2be7412ae",
                                           "Example": "Какой кредитный лимит по карте PLAT/ON?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Максимальная сумма кредита по карте PLAT/ON составляет до 10 000 BYN для физической карты и до 5 000 BYN для виртуальной карты.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "fda95633-fa6e-4857-b7c9-28027c863bf4",
                                           "Example": "Можно ли оформить виртуальную PLAT/ON?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, доступна виртуальная карточка PLAT/ON Classic с кредитным лимитом до 5 000 BYN, которую можно оформить через Интернет-банк.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4439676b-dfc9-4d86-82fd-35f05201f0e6",
                                           "Example": "Когда платеж по карте PLAT/ON?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Обязательный платеж по карте PLAT/ON необходимо вносить до 20 числа каждого месяца.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "cd6a7c68-ee62-4206-aedb-ca881891d9b6",
                                       "Title": "Кредитные карты - Портмоне 2.0",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "61e55208-3f85-4fc8-86fe-d5771f60d8b7",
                                           "Example": "Как оформить кредитную карту Портмоне 2.0?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредитную карту Портмоне 2.0 можно оформить в любом отделении банка. Используется карточка мгновенного выпуска.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "1d07ae3a-7d76-4c94-a903-6e4cfea93a07",
                                           "Example": "Нужны ли справки для карты Портмоне?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредитная карта Портмоне 2.0 оформляется без справок о доходах и поручителей. Необходим только паспорт.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "acf83dc8-9e38-4f7a-a877-a72e40522071",
                                           "Example": "Какой лимит по карте Портмоне 2.0?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредитный лимит по карте Портмоне 2.0 устанавливается индивидуально в зависимости от платежеспособности клиента.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a43edac0-ff45-4ac4-b020-73ea0df38bf7",
                                           "Example": "За сколько выдается карта Портмоне?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Портмоне 2.0 выдается сразу при обращении в отделение банка, так как это карточка мгновенного выпуска.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "b8d640fe-e748-4f23-828a-295a265a6ef7",
                                           "Example": "Какая ставка по карте Портмоне 2.0?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Процентная ставка по карте Портмоне 2.0 указана в тарифах банка. Уточняйте актуальную ставку при оформлении.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "0225f91a-e22a-4f40-b124-75cd62ae8842",
                                           "Example": "Можно ли снимать наличные с Портмоне?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, с кредитной карты Портмоне 2.0 можно снимать наличные в банкоматах, но за эту операцию взимается комиссия согласно тарифам.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "189be451-00a3-4408-b97e-99bb39f7cfe7",
                                       "Title": "Кредитные карты - Отличник",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "777d10af-796e-4a06-adf5-48cac76b5dbf",
                                           "Example": "Кто может оформить карту Отличник?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта Отличник - специальное предложение для действующих кредитополучателей банка ВТБ (Беларусь).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f06b3ef6-81d4-41ac-9464-be4ab60c3628",
                                           "Example": "Нужна ли справка о доходах для Отличника?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Нет, заявка на карту Отличник рассматривается без справки о доходах для действующих клиентов банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "5666c477-9dca-4556-8998-d162ae97f641",
                                           "Example": "Какой процент на остаток по карте Отличник?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "На остаток собственных денежных средств на счете по карте Отличник начисляется 0,01% годовых.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e02a98cf-f36e-4e52-bd3f-28f882a7ef6b",
                                           "Example": "Как погашать задолженность по Отличнику?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Погашение можно осуществлять через Интернет-банк, мобильное приложение, банкоматы ВТБ, отделения банка, через ЕРИП.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "d04b91bb-6eaf-4391-8a0e-b451c4373e4d",
                                           "Example": "Какой кредитный лимит по Отличнику?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Максимальный кредитный лимит по карте Отличник составляет 5 000 BYN на срок до 36 месяцев.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "ffdccd43-9f9e-4365-87f3-dff61ad4db0c",
                                       "Title": "Карты рассрочки - ЧЕРЕПАХА",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "23014098-fcd6-44ac-81dc-c7fe2217d4df",
                                           "Example": "Где можно оформить карту ЧЕРЕПАХА?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карту ЧЕРЕПАХА можно оформить бесплатно в любом офисе банка, онлайн в интернет-банке или мобильном приложении VTB mBank.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "9e917dc0-c380-4d32-9669-287787e4d445",
                                           "Example": "Какой кредитный лимит я могу получить по карте ЧЕРЕПАХА?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "По карте ЧЕРЕПАХА доступен кредитный лимит до 5 000 BYN с возобновляемой линией, которая действует до 3 лет.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "d13c3220-ef77-413a-bda1-581686d3379b",
                                           "Example": "Какие операции доступны по карточке ЧЕРЕПАХА?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "По карте ЧЕРЕПАХА доступны: покупки в рассрочку в магазинах-партнерах, снятие наличных, переводы и покупки в точках по всему миру.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ebee6b9b-7828-4348-b26a-be27d8367d7c",
                                           "Example": "Как узнать текущий долг по ЧЕРЕПАХЕ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Информацию о текущей задолженности можно узнать в личном кабинете интернет-банка или в мобильном приложении VTB mBank в разделе информации по кредиту.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "5ac80519-c778-42d7-b206-2c4b17169699",
                                           "Example": "В каких магазинах принимается ЧЕРЕПАХА?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта ЧЕРЕПАХА принимается в широкой сети магазинов-партнеров. Полный список доступен на сайте cherepaha.vtb.by в разделе 'Магазины'.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "46b97d15-a903-4e9b-824c-8c8e5f43aa2d",
                                           "Example": "Какая рассрочка доступна по ЧЕРЕПАХЕ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "По карте ЧЕРЕПАХА доступна рассрочка до 12 месяцев в зависимости от магазина-партнера и типа товара.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f7b3663a-045a-4e5c-b4cd-36a34cffd10b",
                                           "Example": "Как погашать долг по ЧЕРЕПАХЕ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Погашение можно осуществлять через интернет-банк/мобильное приложение, банкоматы, отделения банка, ЕРИП и переводами с других карт.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2ce6bde9-380a-4a4c-b334-a409b9cd5837",
                                           "Example": "Можно ли пополнить карту ЧЕРЕПАХА?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, карту ЧЕРЕПАХА можно пополнять через банкоматы с функцией Cash-In, в отделениях банка, через интернет-банк.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "6f896639-17fc-42d8-9a06-09181b9af2f5",
                                       "Title": "Карты рассрочки - КСТАТИ",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "ff71d8ca-71f5-422a-a56d-bcde96617969",
                                           "Example": "Как оформить карту КСТАТИ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карту рассрочки КСТАТИ можно оформить в отделениях банка или онлайн через интернет-банк и мобильное приложение VTB mBank.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "fa8097bf-8029-4b41-be65-4b9dcaeffc94",
                                           "Example": "В чем преимущества карты КСТАТИ?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта КСТАТИ предоставляет возможность покупок в рассрочку, удобное управление платежами, широкую сеть партнеров.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a25bb6d4-2398-48e2-abfe-948631a6a6e8",
                                           "Example": "Какие лимиты по карте КСТАТИ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Лимиты по карте КСТАТИ устанавливаются индивидуально в зависимости от платежеспособности клиента.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "6133efee-3a18-42a5-8e19-dafd27f25dfd",
                                           "Example": "Где принимается карта КСТАТИ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Карта КСТАТИ принимается в магазинах-партнерах банка, где доступна рассрочка, а также для обычных покупок.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ac557497-2069-4d54-a205-462babf94d31",
                                           "Example": "Какая рассрочка по карте КСТАТИ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Рассрочка по карте КСТАТИ предоставляется на различные сроки в зависимости от магазина и товара.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8bfadfec-67ad-4c53-8056-81331111ccde",
                                           "Example": "Как узнать баланс карты КСТАТИ?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Баланс и информацию по карте КСТАТИ можно узнать через интернет-банк, мобильное приложение или по телефону контакт-центра.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "fa87ef66-c61e-461c-b6d6-731c1e2c38d1",
                                           "Example": "Можно ли заказать КСТАТИ онлайн?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, карту КСТАТИ можно заказать онлайн через интернет-банк или мобильное приложение VTB mBank.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     }
                                   ]
                                 },
                                 {
                                   "Id": "3f3ca884-9494-4ab0-87de-4570cfe976a0",
                                   "Title": "Продукты - Кредиты",
                                   "SubCategories": [
                                     {
                                       "Id": "042f6090-0097-4f3e-b3ae-902f4095146d",
                                       "Title": "Потребительские - На всё про всё",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "4eed9d73-a0a7-45dd-9052-c8262c544d53",
                                           "Example": "Как оформить кредит На всё про всё?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления кредита 'На всё про всё' обратитесь в любое отделение банка с паспортом. Кредит выдается на любые цели до 70 000 BYN на срок до 7 лет.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8f055294-7d75-4624-8ce0-666696d5c870",
                                           "Example": "Какая максимальная сумма кредита?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Максимальная сумма потребительского кредита 'На всё про всё' составляет 70 000 белорусских рублей.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "d084b7a3-1621-46c3-a00b-c00da45cceeb",
                                           "Example": "Нужны ли справки для кредита?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Требования по документам зависят от суммы кредита. Для небольших сумм возможно оформление без справки о доходах.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "d7d66cae-5ab6-4ef5-932e-da17881ce12b",
                                           "Example": "На какой срок выдается кредит?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредит 'На всё про всё' выдается на срок до 7 лет (84 месяца) в зависимости от суммы и платежеспособности.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ab72a5cf-f7d8-499e-bb53-63adafa7b5aa",
                                           "Example": "Какие документы нужны для кредита?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления кредита необходим паспорт, при необходимости - справка о доходах и документы, подтверждающие цель кредита.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f310ccab-63af-44e5-8090-9723c29653a8",
                                           "Example": "Можно ли досрочно погасить кредит?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, досрочное погашение кредита возможно полностью или частично без штрафов и комиссий.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "cfb02bc9-3717-4b66-ac56-528858c51a95",
                                       "Title": "Потребительские - Дальше - меньше",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "ff2fa8ee-58e8-4c0c-b1dd-21584c5c723b",
                                           "Example": "Что означает снижаемая ставка?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредит 'Дальше - меньше' предоставляется с процентной ставкой, которая ежегодно снижается, делая кредит более выгодным с течением времени.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ed99137a-07a0-4a42-807f-af2dd23f2403",
                                           "Example": "Как работает кредит Дальше - меньше?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Первый год вы платите по стандартной ставке, затем каждый год ставка автоматически снижается на установленный процент.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "17e90896-ef05-4499-a6fa-3a4aad590d07",
                                           "Example": "На сколько снижается ставка каждый год?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Размер ежегодного снижения ставки уточняйте при оформлении кредита, так как он зависит от условий программы.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a1227060-7223-4e19-b2dd-575662b43a3c",
                                           "Example": "Какая первоначальная ставка?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Первоначальная процентная ставка по кредиту 'Дальше - меньше' устанавливается в соответствии с действующими тарифами банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "dc7c83fb-278d-4a9d-b5e3-337998f409b3",
                                           "Example": "Можно ли оформить онлайн?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Заявку на кредит можно подать онлайн через сайт банка, но для окончательного оформления потребуется визит в отделение.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "0b8fb100-bf6e-4afa-b154-85c8a1fdb471",
                                       "Title": "Потребительские - Легко платить",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "9664fc06-a239-4119-8be7-fde0c9cc657c",
                                           "Example": "Что такое льготные месяцы?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Льготные месяцы - это период в начале кредита, когда вы платите по сниженной ставке и можете не погашать основной долг.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "7f89afab-4403-4261-88a8-1243b9b2420d",
                                           "Example": "Как работает кредит Легко платить?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Первые 4 месяца вы платите по сниженной ставке без погашения основного долга, что делает первоначальные платежи минимальными.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "da42ed7c-3227-4d63-83e8-20b865f5bd0a",
                                           "Example": "Сколько месяцев льготный период?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Льготный период по кредиту 'Легко платить' составляет 4 месяца.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "95eff64d-61bf-46ea-b729-7451be1f2590",
                                           "Example": "Какая ставка после льготного периода?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "После окончания льготного периода действует стандартная процентная ставка согласно тарифам банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "63016c1f-a528-4eda-a152-264989bcf1cf",
                                       "Title": "Потребительские - Всё только начинается",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "9dc5f35b-d55a-4f8d-840c-c926afad8609",
                                           "Example": "Кто может оформить этот кредит?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредит 'Всё только начинается' предназначен для клиентов старше 50 лет и предоставляет особые льготные условия.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "29841f97-dfc6-4c72-bb3a-e9bbec574d2e",
                                           "Example": "Какой возраст для кредита 50+?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредит предназначен для граждан, которым исполнилось 50 лет и старше.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8bd890c9-5d04-4579-a2e8-0a55aa99720d",
                                           "Example": "Какие льготы для пенсионеров?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для пенсионеров предусмотрены льготные процентные ставки и упрощенные требования по документам.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e7ed3294-90ca-444c-a448-fba56ea75fe1",
                                           "Example": "Какая максимальная сумма?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Максимальная сумма кредита для клиентов 50+ определяется индивидуально с учетом платежеспособности.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "65148250-c45d-461e-a5eb-d1cebfee6ec0",
                                       "Title": "Потребительские - Старт",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "aa650f37-39a5-4584-a62c-aba9d0314d39",
                                           "Example": "На какую сумму можно взять Старт?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Сумма кредита 'Старт' определяется индивидуально в зависимости от доходов и платежеспособности клиента.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e24b41eb-00d9-4743-ba0f-b0b636a9f18c",
                                           "Example": "Кто может оформить Старт?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредит 'Старт' доступен всем работающим гражданам Республики Беларусь при соответствии требованиям банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "7e011262-5af0-47b4-ae4f-6570189c0100",
                                       "Title": "Онлайн кредиты - Проще в онлайн",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "6d984b72-fb61-4863-b7e4-e53b10bee8a6",
                                           "Example": "Как оформить онлайн кредит?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Онлайн кредит можно оформить через сайт vtb.by или мобильное приложение VTB mBank, заполнив заявку и загрузив необходимые документы.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "7b42927f-6dda-4093-8630-dc2e1346c545",
                                           "Example": "Нужно ли приходить в банк?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "При оформлении онлайн кредита 'Проще в онлайн' посещение банка не требуется - все процедуры проводятся дистанционно.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "61ed3d5b-b3b2-4a1a-85ca-a3c4684e275c",
                                           "Example": "За сколько одобряется онлайн кредит?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Решение по онлайн кредиту принимается в максимально короткие сроки, часто в течение нескольких часов.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "c586ff92-002d-4a32-8027-19ca177966d6",
                                           "Example": "Какие документы загружать онлайн?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для онлайн кредита необходимо загрузить скан паспорта и, при необходимости, справку о доходах.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "0b8ccbdd-8851-4cdb-89e9-abcc469f6f1f",
                                       "Title": "Автокредиты - Автокредит без залога",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "54ae7194-d6e2-40c6-af4c-fa7b0f0bd5fe",
                                           "Example": "На какие авто выдается кредит?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Автокредит без залога выдается на покупку новых и подержанных автомобилей в автосалонах-партнерах банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "5a1b99f9-470b-41f4-baca-f5793fa8dca5",
                                           "Example": "Какая максимальная сумма автокредита?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Максимальная сумма автокредита без залога составляет до 70 000 BYN на срок до 10 лет.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e5624530-e9f2-4021-9ffd-35ed682e89d8",
                                           "Example": "Нужно ли КАСКО для автокредита?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Требования по страхованию КАСКО уточняйте при оформлении автокредита, так как они могут различаться в зависимости от суммы и срока.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2697a159-5bb0-4149-9312-39426dfeb63d",
                                           "Example": "Можно ли купить подержанное авто?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, автокредит без залога доступен как для новых, так и для подержанных автомобилей определенного возраста.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "fb466167-f491-423a-8485-a8f65f4716bd",
                                       "Title": "Экспресс-кредиты - В магазинах-партнерах",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "233e3aa5-b521-4211-a632-a8a565ee781c",
                                           "Example": "В каких магазинах можно оформить?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Условия кредитования зависят от продукта; вы можете найти ставки, сроки и требования в разделе 'Кредиты' на сайте ВТБ (Беларусь).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2a174209-6f6d-43e0-aee5-03a74d30c44e",
                                           "Example": "За сколько одобряется экспресс-кредит?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Экспресс-кредит одобряется в течение нескольких минут прямо в магазине при наличии необходимых документов.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "3046442c-f6e6-4753-9dde-34a6e9e54a53",
                                           "Example": "Какие документы нужны в магазине?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Для оформления экспресс-кредита в магазине достаточно паспорта и, при необходимости, справки о доходах.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "0799059c-83a5-4168-87b8-0a921feedf9b",
                                       "Title": "Экспресс-кредиты - На роднае",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "c7ce9568-1762-4d63-b1d0-892c85506114",
                                           "Example": "Что такое кредит На роднае?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Экспресс-кредит 'На ро́днае' предоставляет льготные условия для покупки товаров, произведенных в Республике Беларусь.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "82a46437-a6d2-4f86-8cea-3808ca5fdf16",
                                           "Example": "На какие товары распространяется?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Кредит 'На ро́днае' действует на товары белорусского производства в магазинах-партнерах банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "3c3955ea-d9ab-4e6c-ab6e-4e921f315f81",
                                           "Example": "Какая льготная ставка?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Льготная процентная ставка по кредиту 'На ро́днае' устанавливается согласно специальным условиям программы.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     }
                                   ]
                                 },
                                 {
                                   "Id": "8c3363a9-a357-45dc-bdf6-a9fc3089e718",
                                   "Title": "Продукты - Вклады",
                                   "SubCategories": [
                                     {
                                       "Id": "ce484b2a-b0f7-4ae7-b320-b7da179b1fe2",
                                       "Title": "Рублевые - Мои условия",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "9f98cf84-ef03-4fea-ad23-aad9d01ae396",
                                           "Example": "Как открыть вклад Мои условия?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Вклад 'Мои условия' можно открыть в любом отделении банка или онлайн через Интернет-банк. Доступны отзывная и безотзывная версии.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ffb6b6a3-7b60-47bc-8852-2e9113f1d353",
                                           "Example": "Какая процентная ставка?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Процентные ставки по вкладам регулярно обновляются. Актуальные ставки размещены на сайте vtb.by в разделе 'Вклады'.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "0e98fb01-e9ad-4003-a62f-fb33623a8d73",
                                           "Example": "Можно ли досрочно закрыть?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Отзывный вклад можно закрыть досрочно без потери процентов. Безотзывный вклад закрывается досрочно с пересчетом процентов.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8ca867ba-dbc2-41de-93f8-c9dcc1d53fd7",
                                           "Example": "Есть ли пополнение вклада?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Условия пополнения зависят от типа вклада. Подробности уточняйте при оформлении договора.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2b59ebe9-c12f-452f-8b9f-623af10c5725",
                                           "Example": "Какая минимальная сумма?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Минимальная сумма для открытия вклада указана в условиях по каждому продукту на сайте банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "8a4be4da-7314-479c-b296-111b3333ca6a",
                                       "Title": "Рублевые - Мои условия онлайн",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "016087d3-ffdf-480e-a4bb-b1eedd55af35",
                                           "Example": "Как открыть онлайн вклад?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Онлайн вклад 'Мои условия онлайн' можно открыть через Интернет-банк или мобильное приложение VTB mBank без посещения отделения.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "849bd621-b7d1-43a1-8177-4cb91f6d8853",
                                           "Example": "Чем отличается от обычного?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Онлайн вклад предлагает повышенные процентные ставки по сравнению с аналогичными продуктами, оформляемыми в отделении.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "3720b8b4-bc7d-4498-9464-da6df8df7ee8",
                                           "Example": "Можно ли управлять через приложение?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, всеми онлайн вкладами можно управлять через мобильное приложение VTB mBank и Интернет-банк.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ad299ae0-6f7c-42d0-bee1-4b8fbb7b6dbb",
                                           "Example": "Как получить деньги при закрытии?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "При закрытии онлайн вклада средства зачисляются на ваш текущий счет, с которого можно получить наличные в любом отделении банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "3135a151-e7e3-4ab5-9a1b-6e451e11f369",
                                       "Title": "Рублевые - Великий путь",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "41aa8e57-de93-4a90-8db6-87b3656a1c8b",
                                           "Example": "Что особенного в этом вкладе?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Вклад 'Великий путь' - это безотзывный депозит с особыми условиями доходности и сроками размещения.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "07f270ad-7027-4787-b615-db3b3e8abd8a",
                                           "Example": "Какие условия по Великому пути?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Подробные условия по вкладу 'Великий путь' уточняйте на сайте банка в разделе вкладов или в отделениях.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "162f3dd3-a424-4310-a9c3-e4710521be4b",
                                           "Example": "Какая доходность?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Доходность условного вклада устанавливается согласно тарифам банка с учетом особых условий продукта.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "a7b79e69-78c1-4a01-89aa-ed6cbcc3f9ff",
                                       "Title": "Рублевые - СуперСемь",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "0affc675-72a5-43d6-bf8a-eb169bf30a75",
                                           "Example": "На какой срок вклад СуперСемь?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Вклад 'СуперСемь' размещается на 7 дней с возможностью автоматической пролонгации.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "7615f827-314f-48de-ac4b-78c29fb56264",
                                           "Example": "Какая ставка по СуперСемь?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Процентная ставка по вкладу 'СуперСемь' указана в действующих тарифах на сайте банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "76bf1201-8c74-4940-ab06-243fd70f3d95",
                                           "Example": "Можно ли открыть онлайн?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, доступна онлайн версия вклада 'СуперСемь-онлайн' через Интернет-банк.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "3486fa7b-4b04-4f03-955f-7f154a679fbb",
                                       "Title": "Рублевые - Подушка безопасности",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "cbf5ab08-4a1c-418e-b537-7857ac866227",
                                           "Example": "Как работает условный вклад?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Условный вклад 'Подушка безопасности' позволяет изымать средства при наступлении определенных жизненных обстоятельств без потери процентов.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "5ecc93fd-b2c7-474a-8182-2721c4d8104d",
                                           "Example": "В каких случаях можно забрать?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Условия досрочного изъятия без штрафов указаны в договоре вклада 'Подушка безопасности'.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f890505b-f060-4b9d-8cbd-78c9bcd75d1b",
                                           "Example": "Какая доходность?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Доходность условного вклада устанавливается согласно тарифам банка с учетом особых условий продукта.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "638121ba-d531-47b5-a5b7-7d3a36274450",
                                       "Title": "Валютные - USD",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "773094c2-8b55-4561-8256-05e8b6570beb",
                                           "Example": "Какая ставка по долларовому вкладу?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Процентные ставки по валютным вкладам в USD регулярно обновляются в соответствии с рыночной ситуацией.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "aeb4f4e4-003a-42ad-9354-5c6b47778013",
                                           "Example": "Какая минимальная сумма в USD?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Минимальная сумма для открытия долларового вклада указана в условиях по валютным депозитам на сайте банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8de941bc-6389-4cd1-beb2-31981fae8533",
                                           "Example": "Можно ли пополнять валютный вклад?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Возможность пополнения валютных вкладов зависит от условий конкретного продукта.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "0300dac3-98c2-4b5a-88bd-25f8c88867a1",
                                       "Title": "Валютные - EUR",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "f11eaf2c-d8f2-4d89-acc0-8fea7be5a3cb",
                                           "Example": "Есть ли вклады в евро?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, банк ВТБ (Беларусь) принимает вклады в евро на различные сроки.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2291d269-6691-4b1d-bb3a-dda7e5a5f8c5",
                                           "Example": "Какие условия по EUR?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Вклады в евро доступны; ставки и условия зависят от выбранного продукта и указаны на странице валютных вкладов.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "38011dcb-a0d3-454b-910f-e9201bc88361",
                                           "Example": "Какая доходность в евро?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Доходность по евровым вкладам устанавливается согласно действующим тарифам банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "b5110931-48b5-43ca-9d39-92f404684844",
                                       "Title": "Валютные - RUB",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "f3ba40e0-98cb-44c3-81bb-10bf21d539e8",
                                           "Example": "Можно ли открыть рублевый вклад?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, банк принимает вклады в российских рублях на различные сроки и условия.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "7a257e33-05b9-4563-87ab-a735630fbe0d",
                                           "Example": "Какая ставка в российских рублях?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Процентные ставки по рублевым вкладам указаны в тарифах банка на сайте vtb.by.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e4ec60df-fcf6-4d5b-89f5-79edfd58bf1e",
                                           "Example": "Есть ли ограничения для RUB?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Ограничения по рублевым вкладам уточняйте при оформлении, так как они могут зависеть от валютного законодательства.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "7e2094da-85aa-4ee4-98c3-64fe642387c3",
                                       "Title": "Валютные - CNY",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "93889949-6130-4e57-9d69-b0b3661a0e24",
                                           "Example": "Принимаются ли юани на вклад?",
                                           "Priority": "средний",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, банк ВТБ (Беларусь) принимает вклады в китайских юанях (CNY).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e3d89ad9-a055-4aa0-b74b-557f68129c5c",
                                           "Example": "Какие условия по китайской валюте?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Вклады в китайских юанях (CNY) доступны; условия (ставки, сроки) публикуются на странице соответствующих валютных вкладов.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "48b8ac57-12c3-41ca-9cf1-924c3a7395a9",
                                           "Example": "Какая доходность CNY?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Доходность по вкладам в китайских юанях устанавливается согласно тарифам банка и рыночной ситуации.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     }
                                   ]
                                 },
                                 {
                                   "Id": "a9edd438-0692-4730-b74d-299d630c3c01",
                                   "Title": "Частные клиенты",
                                   "SubCategories": [
                                     {
                                       "Id": "263ba88a-f103-40c8-a54a-e313de0d54dd",
                                       "Title": "Кредиты",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "ec83ff64-91b8-4f08-aa27-60cfb23e6e6c",
                                           "Example": "Почему банк может отказать в выдаче кредита, и что такое ПДН?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Принятие решения о выдаче кредита происходит на основании анализа данных о клиенте, в том числе расчета его платежеспособности. \n\nПо кредитам на потребительские нужды показатель долговой нагрузки (ПДН) – отношение размера ежемесячного платежа по операциям кредитного характера к размеру среднемесячного дохода. Данный показатель не должен превышать 40%.\n\nТаким образом, банк может отказать в выдаче кредита, если ежемесячная плата по операциям кредитного характера слишком высокая.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e2cbc942-0e12-41f8-94d8-6756a37f890a",
                                           "Example": "Как узнать, закрыт ли кредитный договор?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Получить информацию о закрытии кредитного договора можно:\n- в интернет-банке / мобильном приложении VTB Online;\n- в офисе банка (при себе иметь паспорт) и по звонку по контактным телефонам.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "6629988c-1f30-45fe-b963-24c102bcb583",
                                           "Example": "Могу ли я оформить кредит, если являюсь самозанятым/ремесленником?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, оформление заявки на кредит возможно онлайн либо в офисе банка на сумму, не превышающую 10 000 BYN.\nСамозанятые, уплачивающие единый налог, и ремесленники предоставляют выписку из данных учёта налоговых органов об исчисленных и уплаченных суммах налогов, сборов (пошлин), пеней. \nСамозанятые, уплачивающие налог на профессиональный доход, предоставляют информацию (чеки), подтверждающую уплату налога, сформированную в приложении «Налог на профессиональный доход». \nДокумент (информация), подтверждающий уплату налогов должен содержать сведения за период не менее последних 3 месяцев.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "64ef3dcd-7fa9-4fb9-84ac-5f66912b30c4",
                                           "Example": "Нужна ли справка о доходах для оформления потребительского кредита?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Предоставление справки о доходах не обязательно.\nНо, если ваши доходы выше, чем средняя заработная плата по отрасли трудоустройства, то при предоставлении справки о доходах вероятность получить кредит выше. Бланк справки о доходах.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "f374989a-e57b-40af-9fdf-a2bd678f9de3",
                                           "Example": "Если я оплатил сумму больше, чем мой платеж по графику, спишется ли переплата в счет погашения основного долга автоматически? И нужно ли информировать банк о переплате?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "В данном случае необходимо оформить процедуру частичного досрочного погашения. Это можно сделать одним из следующих способов:\n\n- в интернет-банке / мобильном приложении VTB Online;\n- в офисе банка (при себе иметь паспорт).\n\nПодробнее с информацией о порядке и способах погашения потребительского кредита (в том числе частичного и полного) можно ознакомиться в памятке.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "1e729096-a080-46a1-b5f5-37cb1239744c",
                                           "Example": "Можно ли получить кредит онлайн?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Банк ВТБ (Беларусь) предлагает онлайн-оформление потребительского кредита, универсальной карточки рассрочки Черепаха, кредитной карты Портмоне 2.0 или PLAT/ON. Доступная сумма кредитного лимита будет рассчитываться исходя из дохода и действующих кредитов/рассрочек. Окончательное решение принимается после оформления заявки в интернет-банке / мобильном приложении VTB Online.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ffaa9c8f-0797-4a9c-a3d4-8daed52cae1b",
                                           "Example": "Можно ли оформить кредит, находясь в декретном отпуске?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "К сожалению, нет. По условиям кредитных программ, заявитель не должен находиться в социальном отпуске по уходу за ребенком до достижения им возраста 3-х лет.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "24d702f9-8123-4231-a401-abd1553b189a",
                                           "Example": "Могу ли я взять кредит, если официально работаю за рубежом?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Да, оформление заявки на кредит возможно на сумму, не превышающую 15 000 BYN.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a97e58e0-b978-4682-b0b4-ea82d78f7b71",
                                           "Example": "Могу ли я получить кредит на потребительские нужды, если являюсь ИП?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Да, оформление заявки на кредит возможно.\nДоступная сумма кредита будет рассчитываться исходя из дохода индивидуального предпринимателя и действующих кредитов, рассрочек.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e6df6d8f-2d34-432a-989c-6b3ee9bc9be6",
                                           "Example": "Можно ли оформить в ВТБ (Беларусь) кредит пенсионеру?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Банк ВТБ (Беларусь) предлагает оформление кредита наличными или на карту, при этом возраст заявителя по ряду кредитных программ должен быть не более 75 лет на момент окончания кредитования.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ec94ca3d-31cf-41e0-ae8e-c646a554d352",
                                           "Example": "Какая максимальная сумма беззалогового кредитного продукта?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Максимальная сумма беззалогового кредитного продукта – 200 000 белорусских рублей.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2364c9c1-6c0b-4338-bb0f-5a0318f3ce81",
                                           "Example": "Существует ли в банке возрастное ограничение к физическим лицам, зарегистрированным в качестве индивидуального предпринимателя?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "На момент подачи заявки на кредитный продукт возраст индивидуального предпринимателя должен быть от 25 лет и до 64 лет (включительно).\n\n",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "1ef717e6-3464-43ee-a863-e2e4f9ff0147",
                                           "Example": "В каких валютах можно получить кредит в банке?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "ВТБ (Беларусь) предоставляет кредиты в белорусских рублях (BYN), российских рублях (RUB), долларах США (USD), евро (EUR) и китайских юанях (CNY).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a69d7903-27cd-42dd-ac1c-156188b2983c",
                                           "Example": "Возможно ли дистанционно направить в банк заявку на кредит?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Да, такая возможность предусмотрена для клиентов малого бизнеса (ИП и организаций), подключенных к системе Интернет-Банк.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "6ea5b16c-7ed9-44ad-840f-2a1edb7ff53f",
                                           "Example": "Возможно ли обратиться в банк с заявкой на кредит, если с даты государственной регистрации организации прошло 7 месяцев?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Да, возможно. Для клиентов малого бизнеса, срок деятельности которых от 6 до 12 месяцев (исчисляется с даты государственной регистрации до дня обращения в банк), банк предлагает кредитные продукты под обеспечение в виде залога (гарантийного депозита денег, поручительство государственных фондов) в размере не менее 50% от суммы кредитного продукта. ",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "7be5272f-5f08-4829-9b0e-0a1b62180fb4",
                                           "Example": "Использование системы ЕРИП для погашения",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Погашение кредитов и пополнение счетов можно произвести через систему ЕРИП (выберите услугу банка ВТБ (Беларусь) и введите номер договора).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e4abe08e-bcf2-424c-a270-416f536ed7bc",
                                           "Example": "USSD-запросы для получения информации о платеже",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "USSD-сервисы доступны: формат запроса для информации о платеже по кредиту — *130*7*<номер_договора>#. Полный перечень USSD-служб указан в разделе 'USSD-сервисы'.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "b754ae2f-28b6-4d05-bd47-ecff7666800b",
                                           "Example": "Если у кредитополучателя была смена фамилии и паспорта, надо ли сообщать об этом в банк? Есть кредит в Вашем банке, но недавно у меня изменился номер мобильного телефона. Как можно проинформировать банк?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Для актуализации паспортных данных Вам необходимо обратиться в любой офис банка ВТБ с паспортом и свидетельством о браке. Адреса и режим работы офисов указаны на сайте банка https://www.vtb.by/regionalnaya-set.\n\nДля актуализации контактных данных Вам необходимо обратиться в любой офис банка ВТБ с паспортом.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "fbc4cc25-abb3-4e8c-a8d8-4b45c9ae35a9",
                                           "Example": "Как узнать сколько нужно платить по кредиту?\n",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Информация о минимальной обязательной сумме ежемесячного платежа:\n\n- доступна в интернет-банке / мобильном приложении VTB Online;\n- доступна посредством запроса «USSD-предстоящий платеж по кредиту» (для абонентов А1 и МТС) следующего формата: *130*7*ХХХХХХХХХ#вызов, где ХХХХХХХХХ – платежный номер по кредиту. Запрос должен быть выполнен кредитополучателем с мобильного телефона, предоставленного в банк вместе с иными клиентскими данными (первоначально в заявлении-анкете);\n- доступна к получению в офисе банка (при себе иметь паспорт) и по звонку по контактным телефонам.\n\nПодробнее с информацией о порядке и способах погашения потребительского кредита (в том числе частичного и полного) можно ознакомиться в памятке.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "cec7693e-18c3-4909-a62e-33c4fb15ccc8",
                                           "Example": "Взимается ли плата за зачисление кредитных средств на текущий счет клиента либо по реквизитам, указанным клиентом?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Предоставление кредита (зачисление на текущий (расчетный) банковский счет либо перечисление денежных средств по указанным клиентом реквизитам) осуществляется бесплатно.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "7439b124-c0a2-4aca-90d8-319665b2d7bc",
                                           "Example": "Могу ли я отозвать свое согласие на предоставление кредитного отчета?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Согласие на предоставление кредитного отчета может быть отозвано кредитополучателем в порядке, определяемом Национальным банком Республики Беларусь, за исключением случаев наличия действующей кредитной сделки между банком и кредитополучателем.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e4a44201-0651-4a58-b0d9-e6bdfa1ad7f7",
                                           "Example": "С какой целью при оформлении кредита открывается текущий счет? И нужно ли платить за его открытие?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "При оформлении потребительского кредита клиенту предлагается заключить два вида договоров: кредитный договор и договор текущего (расчетного) банковского счета. \nТекущий (расчетный) банковский счет используется не только для зачисления кредитных денежных средств, но и для погашения кредита: ежемесячно клиенту необходимо вносить на данный счет сумму средств, достаточную для уплаты ежемесячного платежа по кредиту. \nОткрытие текущего (расчетного) банковского счета в рамках кредитных программ банка осуществляется бесплатно.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "5c035269-efa5-4b0e-b2be-c07dbc84636e",
                                       "Title": "Банковские карточки",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "ac9c341f-747b-4fbc-9890-28d4e6476e75",
                                           "Example": "Как узнать текущий платеж по кредитной карточке?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Информация о минимальной обязательной сумме ежемесячного платежа:\n- доступна в интернет-банке / мобильном приложении VTB Online;\n- доступна посредством запроса «USSD-предстоящий платеж по кредитной карточке» (для абонентов А1 и МТС) следующего формата: *130*7*ХХХХХХХХХ#вызов, где ХХХХХХХХХ – платежный номер по кредитной карточке. Запрос должен быть выполнен кредитополучателем с мобильного телефона, предоставленного в банк вместе с иными клиентскими данными (первоначально в заявлении-анкете);\n- доступна к получению в офисе банка (при себе иметь паспорт) и по звонку по контактным телефонам.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "62626a45-16cd-41e8-9045-9320531f3336",
                                           "Example": "Как можно бесплатно и мгновенно пополнить платежную карточку банка?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "1. Через участников системы АИС \"Расчет\" (маршрут в ЕРИП: Банковские, финансовые услуги - Банки НКФО - Банк ВТБ (Беларусь) - Пополнение карточек и Е+PAY). 2. Переводом с другой карточки банка ВТБ (Беларусь), с другой карточки банков-резидентов Республики Беларусь и с карточек платежной стсемы \"Мир\" из иных стран. 3. В отделениях банка ВТБ (Беларусь).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "790b1f95-771a-43d9-ae14-ef20f1ef05a8",
                                           "Example": "Как быстро можно получить карточку «Мир»?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "При обращении в офис Банка ВТБ (Беларусь), выдается карта мгновенного выпуска, сразу при обращении клиента. При заказе  в интернет-банке / мобильном приложении VTB Online  карточка изготавливается именная (срок изготовления и доставки от 3-х дней). Также в интернет-банке / мобильном приложении VTB Online  можно выпустить виртуальную  карточку  «Мир»  в  BYN ,  например карточку MORE (оформление  и выпуск займет не более 5 минут).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "6bd1de69-ed81-46e8-b0ff-2e42d73fec15",
                                           "Example": "Можно ли получить карточку почтой?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "При заказе карты в интернет-банке (включая мобильное приложение VTB Online), можно выбрать способ получения карточки (в офисе банка или доставка почтой). Доставка осуществляется только по территории Республики Беларусь.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "2641c7a1-84c7-48e1-a212-7ee0c10fb718",
                                           "Example": "С какого возраста можно оформить платежную карточку в банке?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Оформление карточки возможно начиная с 14 лет, при наличии документа, удостоверяющего личность.\n\nВАЖНО. В случае оформления карточки лицом, не достигшим восемнадцатилетнего возраста, необходимо согласие законного представителя.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ec1ef152-225c-4122-bb52-78729dcc466b",
                                           "Example": "Как активировать карточку?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Необходимо совершить первую успешную операцию с вводом ПИН-кода, например совершить запрос баланса в  банкомате любого банка (один запрос в месяц бесплатный).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "efd2ab94-e30b-4a4c-b5d4-dd9f59a7d98d",
                                           "Example": "Обязательно ли обращаться в офис банка для оформления платежной карточки?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Не обязательно — многие карты можно заказать онлайн через интернет-банк или мобильное приложение; получение пластика может потребовать посещения отделения или курьерскую доставку.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ee4756b8-ff94-491b-a2a7-11c85998249b",
                                           "Example": "Какую карточку оформить, чтобы оплачивать товары на OZON, Wildberries?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Банк предлагает оформление карточек платежной системы «Мир» и Белкарт, которыми можно рассчитываться на интернет-ресурсах, принимающих карточки «Мир», в том числе OZON, E-dostavka, Wildberries и других торговых площадках с  размещенным логотипом «Мир» и/или Белкарт.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4f1af9b8-8491-4e30-ab39-8622754ebd7c",
                                           "Example": "Как можно бесплатно пополнить карточку MORE?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Через участников системы АИС «Расчёт» (ЕРИП), в том числе отделения РУП «Белпочта».\nБанковским переводом внутри страны, в том числе от организаций без заключения договора на зарплатное обслуживание. Переводом с другой карточки банка ВТБ (Беларусь) и с карточек ПС «Мир» из иных стран. В отделениях банка ВТБ (Беларусь).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "1fc1ece8-6011-4ccf-b97b-b367add02b59",
                                           "Example": "Карты каких платежных систем можно использовать в Mir Pay?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "В приложение Mir Pay можно добавить любую карточку «Мир» ЗАО Банк ВТБ (Беларусь).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "091c8560-002c-4b48-9212-7a5b8291cdf5",
                                           "Example": "Сколько стоит сервис Mir Pay?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Данный сервис абсолютно бесплатный для всех клиентов — вам нужна только активная карта «Мир» ЗАО Банк ВТБ (Беларусь). При оплате покупок дополнительная комиссия также не взимается.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "16a1f505-a96d-4010-8c18-d319c3c7cee9",
                                           "Example": "Какие устройства совместимы с Mir Pay?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Mir Pay поддерживается только смартфонами на базе OC Android. Обязательными условиями являются поддержка технологии NFC и предустановленная версия ОС Android не ниже 7.0.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "423a7bbe-4db5-46c7-a7dc-f7cea3af94ac",
                                           "Example": "Рекомендации по безопасному использованию карточек",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Можете ознакомиться с рекомендациями по данной ссылке.\nhttps://www.nbrb.by/today/FinLiteracy/Consumer/recomend_card.pdf",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "bed1e37d-d572-4a8c-90c6-a85b48c779e2",
                                           "Example": "Действия при утрате карточки",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "При утрате карточки банка «ВТБ (Беларусь)» рекомендуется незамедлительно заблокировать её любым доступным способом:\n- через интернет-банк или М-банкинг;\n- с помощью услуги «USSD-блокировка» (нужно отправить с номера мобильного телефона, предоставленного ранее в банк, USSD-команду 13021XXXX# вызов, где XXXX — последние четыре цифры номера карточки);\nпо телефонам: +375 17 309 15 15, +375 29 309 15 15, +375 33 309 15 15.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "b97c81e1-9a6f-457d-881b-393412c76676",
                                           "Example": "Я потерял пин-код к карточке. Как его восстановить?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Вы можете осуществить смену ПИН-кода карточки бесплатно в сервисе Интернет-банк/ М-банкинг. Необходимо в разделе \"Карточки\" выбрать карточку и перейти по ссылке \"Смена ПИН-кода\".\n\nТакже можно сменить ПИН-код, позвонив в банк по телефону: +375 (17/ 29/ 33) 3091515, в данном случае плата за смену ПИН-кода составит 3 рубля/ 2 доллара США либо евро/ 100 российских рублей.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4d7c4330-5795-499e-a0ab-a9adad9721e3",
                                           "Example": "Как отключить SMS-информирование по карте без обращения в банк?\n",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Отключить услугу SMS- информирование можно в сервисе \"Интернет-банк\"/\"М-банкинг\". Для этого необходимо в разделе \"Карточки\" выбрать карту, затем вкладку \"Сервисы\" и перейти по ссылке \"SMS-информирование\", в данном случае услуга бесплатная.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8ec281e9-9a2a-4506-af88-f6299c901772",
                                           "Example": "У меня карта российского банка, могу ли я снять деньги в банкомате Банка ВТБ (Беларусь)?\n",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Снять наличные средства можно в белорусских рублях в банкоматах ЗАО Банк ВТБ (Беларусь) по карточкам платежной системы МИР. Комиссия со стороны ЗАО Банк ВТБ (Беларусь) за снятие наличных не взимается, при этом установлен лимит по снятию наличных по одной карточке в размере до 50 (пятидесяти) базовых величин Республики Беларусь в месяц. Рекомендуем Вам также уточнить тарифы и ограничения, установленные банком-эмитентом Вашей карточки.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a66a0b89-cfe5-48c4-b6ac-11da1ec1d617",
                                           "Example": "Каким способом можно пополнить карточку, оформленную в Банке ВТБ (Беларусь), находясь в Москве?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Пополнить карточку, эмитированную ЗАО Банк ВТБ (Беларусь), можно следующими способами:\n\n1) Посредством перевода с карты на карту. Комиссию за зачисление на счет Банк ВТБ (Беларусь) не взимает;\n\n2) Оформить банковский перевод из банка-нерезидента. За зачисление денежных средств со счетов, открытых в иных банках, на Ваш счет, Банком ВТБ (Беларусь) взимается плата согласно Сборнику плат (Раздел 8, пункт 8.1.7.).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "8268d33a-ea07-43cf-8475-c36e01bf5934",
                                           "Example": "Как быстро можно получить карточку «Мир» или Белкарт?\n",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "При обращении в офис Банка ВТБ (Беларусь), выдается карта мгновенного выпуска, сразу при обращении клиента.\n\nПри заказе  в интернет-банке / мобильном приложении VTB Online  карточка изготавливается именная (срок изготовления и доставки от 3-х дней).\n\nТакже в интернет-банке / мобильном приложении VTB Online  можно выпустить виртуальную  карточку  «Мир»  в  BYN ,  например карточку MORE (оформление  и выпуск займет не более 5 минут).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "c5121303-deff-45e4-8ae8-edeab44c89b8",
                                       "Title": "Вклады и депозиты",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "81fb3b7e-082f-4cef-a68e-84d5b6a82978",
                                           "Example": "Могу ли я оформить онлайн-вклад в вашем банке?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Банк ВТБ (Беларусь) предлагает своим клиентам открытие безотзывных вкладов \"Мои условия онлайн\" и \"СуперСемь-онлайн\" в интернет-банке / мобильном приложении VTB Online. Для этого необходимо иметь текущий счет либо счет с карточкой, открытый в нашем банке. Валюта данного счета и валюта вклада должны совпадать.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "d784da17-7b78-46e9-82de-6d69532e572c",
                                           "Example": "Обязательно ли приходить в офис банка для открытия депозитного счета?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "При наличии открытого текущего (расчетного) счета в ВТБ (Беларусь) вы можете открыть депозитный счет прямо в Интернет-банке. Посещать офис банка для этого не нужно.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "14baea41-1ac3-4486-a7df-5ec2cc8765ef",
                                           "Example": "Какой регламент размещения онлайн-депозитов?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "С регламентом размещения депозитов вы можете ознакомиться по ссылке: https://www.vtb.by/korporativnym-klientam/raschetno-kassovoe-obsluzhivanie-dlya-korporativnyh-klientov",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "ac45ab68-8f5a-45c7-b26b-710e99712d07",
                                           "Example": "Возможно ли перечислить первоначальный взнос в депозит в сумме, которая будет выше первоначального взноса, указанного в депозитном договоре?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, возможно. Сумма денежных средств, перечисленных сверх суммы первоначального взноса, указанной в депозитном договоре, является дополнительным взносом в депозит. Он будет размещен в рамках условий депозитного договора.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "96852815-885e-491e-84a5-5fd372660256",
                                           "Example": "Возможно ли оформить депозит, не посещая офис банка?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, возможно. В Интернет-банке и приложении VTB Business вы можете оформить отзывный или безотзывный депозит онлайн. Разместить денежные средства можно в белорусских или российских рублях, а также китайских юанях.\n\n",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "4b132e5c-b74f-45d5-a6b4-2143d12fb08b",
                                           "Example": "Возможно ли оформить депозит с перечислением денежных средств из другого банка?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Да, возможно. Вы можете воспользоваться опцией размещения депозита в Интернет-банке с выбором варианта перечисления первоначального взноса из другого банка.\n\n\n",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "3e13f768-3af7-4392-a4fd-4a46694e7378",
                                           "Example": "При оформлении вклада в каком случае нужно платить подоходный налог?",
                                           "Priority": "высокий",
                                           "Audience": "новые клиенты",
                                           "Answer": "Оплата подоходного налога процентных доходов, полученных по банковским вкладам зависит от фактического срока хранения денежных средств на счете вклада (депозита). Подлежат налогообложению процентные доходы по тем денежным средствам, фактический срок размещения которых на счете банковского вклада (депозита) составляет менее одного года в белорусских рублях, а в иностранных валютах менее двух лет. ",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "0325fd99-1714-4911-9c69-2d3ccc64ca3d",
                                           "Example": "Мне необходимо самому производить оплату подоходного налога в налоговый орган?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Банк является налоговым агентом, поэтому проценты по вкладу начисляются клиенту уже за вычетом подоходного налога.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "28e64cff-232b-4622-aeed-3c7e4ab9407b",
                                           "Example": "Где можно получить наличные после окончания онлайн-вклада?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Получить наличные денежные средства возможно в любом офисе банка. Для Вашего удобства необходимо в интернет-банке / мобильном приложении VTB Online осуществить дистанционный заказ денежной наличности. Заказ наличности в интернет-банке / мобильном приложении VTB mBank доступен по онлайн-вкладам не ранее чем за 15 дней до срока окончания вклада. Необходимо в личном кабинете выбрать Вклад далее пункт «Заказать денежную наличность» - ввести сумму для заказа и выбрать офис (ДО или РД) – подтвердить действия сеансовым паролем. В дальнейшем с Вами свяжется специалист офиса по номеру, имеющемуся в банке. ",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "264aa4b8-3583-4b7a-b776-0533ca60fe7c",
                                           "Example": "Может ли нерезидент Беларуси открыть вклад в вашем банке?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Нерезидент Республики Беларусь может открыть вкладной счет в ЗАО Банк ВТБ (Беларусь) и разместить безотзывный и отзывный вклад на предлагаемые сроки. Необходимо лично обратиться в любое отделение банка с паспортом. Также можно дистанционно открыть онлайн-депозит в интернет-банке / мобильном приложении VTB Online, при этом необходимо иметь текущий счет либо счет с карточкой, открытый в Банке ВТБ (Беларусь). Валюта данного счета и валюта вклада должны совпадать.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "95607728-24f0-430f-a485-06404d2d3d14",
                                           "Example": "Могу ли я закрыть вклад, размещенный в офисе банка ВТБ (Беларусь) расположенном в Минске, находясь в другом городе?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Закрытие вкладного счета в дату окончания вклада банк осуществляет без предоставления вкладчиком дополнительных платежных инструкций. Оформить заявление на досрочное закрытие отзывного вклада возможно в любом офисе банка на всей территории Республики Беларусь. При себе Вам необходимо иметь паспорт. Это касается также и операций пополнения либо снятия денежных средств со счета в офисе банка.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "31ae75ba-59b1-47c4-b548-14dede861179",
                                           "Example": "Могу ли я по телефону узнать дату окончания вклада?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Дату окончания вклада можно узнать в личном кабинете в интернет-банке / мобильном приложении VTB Online в разделе Депозиты. Также информация по вкладному счету может быть предоставлена вкладчику (доверенному лицу) при личном обращении в офис банка ВТБ (Беларусь).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "0954d107-d9d6-4c60-aada-a3ea2f44c7bd",
                                           "Example": "На какие пункты договора вклада стоит обращать особое внимание?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "В банковском кодексе прописан ряд существенных условий, которые должны быть отражены в обязательном порядке в договоре банковского вклада. Это фамилия, имя, отчество вкладчика; валюта вклада и сумма первоначального взноса; размер процентной ставки; срок возврата вклада и некоторые другие.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "5a78ff42-8502-44ce-b258-cb26b2be1f5d",
                                           "Example": "Что делать если банк обанкротился? Что такое возмещение по вкладам?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Государство гарантирует сохранность денежных средств, размещенных на счетах и (или) в банковские вклады (депозиты) в банках Республики Беларусь для физических лиц, в том числе выступавших в качестве индивидуальных предпринимателей, в соответствии с:\nЗаконом Республики Беларусь от 8 июля 2008 г. № 369-З “О гарантированном возмещении банковских вкладов (депозитов) физических лиц” (в ред. Закона Республики Беларусь от 11.11.2021 г. № 128-З)\nДекретом Президента Республики Беларусь от 4 ноября 2008 г. № 22 “О гарантиях сохранности денежных средств физических лиц, размещенных на счетах и (или) в банковские вклады (депозиты)” (в ред. Декрета Президента Республики Беларусь от 31.01.2022 г. № 1)",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "a10d9d3b-e381-4d93-a258-84a8411d6b85",
                                           "Example": "Капитализация процентов ",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Это дополнительное условие по вкладу связанное с присоединением суммы начисленных процентов (как правило,1 раз в месяц) к сумме договора. В результате в следующем месяце при расчете суммы процентов будет использоваться большая расчетная сумма (сумма вклада+ сумма % за предыдущий месяц), что позволяет в итоге получать больший размер дохода по вкладу",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     },
                                     {
                                       "Id": "7c112f58-6815-4dd0-9884-e9dc0c0e613c",
                                       "Title": "Онлайн-сервисы",
                                       "CategoryId": "00000000-0000-0000-0000-000000000000",
                                       "Category": null,
                                       "Questions": [
                                         {
                                           "Id": "325a3882-c1b0-436c-bfd1-cc002c4ba918",
                                           "Example": "Почему не получается войти под логином и паролем из старого ДБО?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Новые системы Мобильного и Интернет-банка базируются на разных базах зарегистрированных клиентов. Вам потребуется пройти процедуру регистрации в системе при первом использовании системы.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "efb0a36c-d8c7-4abb-86f2-0d2956712036",
                                           "Example": "Система пишет, что такого клиента нет. Что делать?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Перед началом использования системы требуется пройти процедуру регистрации. Это требование распространяется на всех Клиентов Банка, вне зависимости от того, использовали они старые системы Мобильного и Интернет-банка ранее или нет.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "334f3a45-294c-481a-ad82-950c013d6e6b",
                                           "Example": "А логин и пароль в новом Интернет-банке и Мобильном банке отличаются?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Нет, Клиент может, используя одну комбинацию логина и пароля, получить доступ к Мобильной и Интернет версии новой системы.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "3353788c-413d-442a-93bb-23b94d061dd1",
                                           "Example": "Есть ли возможность принимать платежи из-за границы?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Платежи сервиса E-POS доступны только на территории Республики Беларусь.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "e828b3ed-df7d-4c02-8fc0-db83ffd30852",
                                           "Example": "Где получить информацию о платежах за месяц?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "В личном кабинете E-POS реализован сервис формирования и отправки отчетов на e-mail за любой период, который укажет клиент.",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         },
                                         {
                                           "Id": "cc871c7c-a896-43e4-8fcb-78e2bbfa7f11",
                                           "Example": "Потребуется ли какое-то специальное оборудование или программное обеспечение для подключения к сервису e-pos?",
                                           "Priority": "средний",
                                           "Audience": "все клиенты",
                                           "Answer": "Специальное оборудование и разработка программного обеспечения не требуются. Достаточно иметь устройство с выходом в интернет (компьютер, ноутбук, планшет).",
                                           "SubCategoryId": "00000000-0000-0000-0000-000000000000",
                                           "Embedings": [],
                                           "SubCategory": null
                                         }
                                       ]
                                     }
                                   ]
                                 }
                               ]
                               """;
}