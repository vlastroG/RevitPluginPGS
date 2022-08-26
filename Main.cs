using Autodesk.Revit.UI;
using MS.Utilites;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;


namespace MS
{
    public class Main : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {

            CreatePanel(application);


            return Result.Succeeded;
        }

        /// <summary>
        /// Создание панели
        /// </summary>
        /// <param name="application"></param>
        public static void CreatePanel(UIControlledApplication application)
        {
            // Create Ribbon tab
            var path = Assembly.GetExecutingAssembly().Location;

            var assembly_dir = WorkWithPath.AssemblyDirectory;

            // Создание вкладки в ленте
            application.CreateRibbonTab("PGS-BIM");
            // Инфо раздел
            RibbonPanel panelGeneral = application.CreateRibbonPanel("PGS-BIM", "Общее");
            // Раздел АР
            RibbonPanel panelAR = application.CreateRibbonPanel("PGS-BIM", "Раздел АР");
            //Раздел КР
            RibbonPanel panelKR = application.CreateRibbonPanel("PGS-BIM", "Раздел КР");
            // Раздел СС
            // RibbonPanel panelSS = application.CreateRibbonPanel("PGS-BIM", "Раздел СС");
            // Раздел ОВиК
            RibbonPanel panelOVVK = application.CreateRibbonPanel("PGS-BIM", "Раздел ОВиК");


            // Info command
            PushButtonData btnInfo = new PushButtonData("Info", "Info", path, "MS.Info");
            Uri btnInfoImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnInfoImage = new BitmapImage(btnInfoImagePath);
            btnInfo.LargeImage = btnInfoImage;
            btnInfo.ToolTip = "Информация о плагине";


            // LevelName command
            //PushButtonData btnLevelName = new PushButtonData("LevelName", "Отметки\nуровней", path, "MS.Commands.General.LevelName");
            //Uri btnLevelNameImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            //BitmapImage btnLevelNameImage = new BitmapImage(btnLevelNameImagePath);
            //btnLevelName.LargeImage = btnLevelNameImage;
            //btnLevelName.ToolTip = "Проверка отметок уровней в их названии";


            // SelectionRooms command
            PushButtonData btnSelection = new PushButtonData("Selection", "Выбор\nпомещений", path, "MS.Selector");
            Uri btnSelectionImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Selection.png");
            BitmapImage btnSelectionImage = new BitmapImage(btnSelectionImagePath);
            btnSelection.LargeImage = btnSelectionImage;
            btnSelection.ToolTip =
                "Интерфейс переключается в режим выбора рамкой только элементов категории \'Помещения\'.";
            btnSelection.LongDescription =
                "За одно использование команды можно выделить только помещения только рамкой только один раз.";

            // MasonryMesh command
            PushButtonData btnMasonryMesh = new PushButtonData("MasonryMesh", "Кладочная\nсетка", path, "MS.Commands.AR.MasonryMesh");
            Uri btnMasonryMeshImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Selection.png");
            BitmapImage btnMasonryMeshImage = new BitmapImage(btnMasonryMeshImagePath);
            btnMasonryMesh.LargeImage = btnMasonryMeshImage;
            btnMasonryMesh.ToolTip = "Расчет длины кладочной сетки в пог.м. для однослойных стен " +
                "с заполненным параметром \'PGS_ТипАрмирования\' (1 или 2) " +
                "и назначение \'Мрк.НаименованиеСетки\' по заполненным параметрам.";
            btnMasonryMesh.LongDescription = "Длина кладочной сетки расчитывается следующим образом:" +
                "\n\tДлина стены * PGS_АрмКолвоАрмРядов - Длина кладочной сетки в проемах" +
                "\nДлина кладочной сетки в проемах расчитывается:" +
                "\n\tВысота проема / Высота стены * PGS_АрмКолвоАрмРядов * Ширина проема. " +
                "\nКоличество армируемых рядов в проеме округляется до меньшего целого." +
                "\n\nЕсли \'PGS_ТипАрмирования\' = 1, то наименование сетки компонуется из " +
                "\'Мрк.МаркаСетки\', \'PGS_АрмДиаметр\', \'Арм.КлассСтали\', \'PGS_АрмШаг\' и " +
                "\'PGS_АрмОтступОтГраней\'." +
                "\n\tПример: С-1 5Вр1-50/5Вр1-50 23" +
                "\nЕсли \'PGS_ТипАрмирования\' = 2, то наименование компонуется из " +
                "\'PGS_АрмДиаметр\' и \'Арм.КлассСтали\'." +
                "\n\tПример: Ø6 А240, м.п.";

            // RoomsAreaPGS command
            PushButtonData btnRmArea = new PushButtonData("RoomsAreaNew", "Квартирография\nPGS_temp", path, "MS.Commands.AR.RoomsAreaNew");
            Uri btnRmAreaImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\RoomsArea.png");
            BitmapImage btnRmAreaImage = new BitmapImage(btnRmAreaImagePath);
            btnRmArea.LargeImage = btnRmAreaImage;
            btnRmArea.ToolTip = "Расчет количества жилых комнат в квартирах, " +
                "жилой площади квартиры, отапливаемой и общей с коэффициентом. " +
                "Для маркировки квартир можно пользоваться командой \'Выбор помешений\'.";
            btnRmArea.LongDescription = "Квартиры компонуются по значению параметра \'ADSK_Номер квартиры\', " +
                "это и Имя помещения - единственные параметры, которые необходимо корректно заполнить." +
                "Тип комнат (и коэффициент площади) определяется по их Именам, " +
                "ознакомиться со списком и внести в него корректировки " +
                "можно в BIM-отделе. Полученные значения будут записаны в параметры \'ADSK_Количество комнат\', " +
                "\'ADSK_Площадь квартиры жилая\', \'ADSK_Площадь квартиры\' и \'ADSK_Площадь квартиры общая\'.";

            // ComplexApartmentNumber command
            PushButtonData btnCmplxAprtNum = new PushButtonData("ComplexAprtmntNmbr", "Составные номера\nквартир", path, "MS.Commands.AR.ComplexAprtmntNmbr");
            Uri btnCmplxAprtNumImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnCmplxAprtNumImage = new BitmapImage(btnCmplxAprtNumImagePath);
            btnCmplxAprtNum.LargeImage = btnCmplxAprtNumImage;
            btnCmplxAprtNum.ToolTip = "Заполнение составного номера квартиры в Комментирии";
            btnCmplxAprtNum.LongDescription = "Составной номер будет компоноваться из значений параметров " +
                "\'ADKS_Количество комнат\', \'ADSK_Тип квартиры\' и \'ADSK_Индекс квартиры\'." +
                " Полученное значение будет записано в параметр \'Комментарии\'";

            // OpeningsArea command
            PushButtonData btnOpeningsArea = new PushButtonData("OpeningsArea", "Площадь проемов\nв помещениях", path, "MS.Commands.AR.OpeningsArea");
            Uri btnOpeningsAreaImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnOpeningsAreaImage = new BitmapImage(btnOpeningsAreaImagePath);
            btnOpeningsArea.LargeImage = btnOpeningsAreaImage;
            btnOpeningsArea.ToolTip = "Площади проемов помещаний будут записаны в параметр \'ADSK_Площадь проемов\'";
            btnOpeningsArea.LongDescription = "Площади проемов расчитываются с учетом расположенных в помещении окон, " +
                "дверей, витражей и линий границ помещений. " +
                "В исключительных случаях моделирования площади проемов могут считаться некорректно. " +
                "В основном это происходит, если у помещения необычная геометрия " +
                "и его границы сделаны границей помещений; или если в помещении переменный уровень пола." +
                "Такие уникальные помещения нужно убрать из расчета во всплывающем окне.";

            // MarkLintelsInOpenings command
            PushButtonData btnMarkLintelsInOpenings = new PushButtonData("MarkLintelsInOpenings", "Маркировать перемычки\nокон и дверей", path, "MS.Commands.AR.MarkLintelsInOpenings");
            Uri btnMarkLintelsInOpeningsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnMarkLintelsInOpeningsImage = new BitmapImage(btnMarkLintelsInOpeningsImagePath);
            btnMarkLintelsInOpenings.LargeImage = btnMarkLintelsInOpeningsImage;
            btnMarkLintelsInOpenings.ToolTip = "Назначение Марок перемычек в параметры окон и дверей: " +
                "\'PGS_МаркаПеремычки\' и \'Мрк.МаркаКонструкции\', и назначение массы перемычки " +
                "(\'ADSK_Масса элемента)\' в \'PGS_МассаПеремычки\' у окна и двери при помощи выпадающего окна.";
            btnMarkLintelsInOpenings.LongDescription =
                "В Описании типоразмера родительского семейства перемычки (и только в нем!) " +
                "должно быть написано \'Перемычка\'." +
                "Семейство перемычки должно быть выполнено категорией \'Обобщенные модели\'.";

            // LintelsSections command
            PushButtonData btnLintelsSections = new PushButtonData("LintelsSections", "Разрезы\nпо перемычкам", path, "MS.Commands.AR.CreateSectionsByLintels");
            Uri btnLintelsSectionsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnLintelsSectionsImage = new BitmapImage(btnLintelsSectionsImagePath);
            btnLintelsSections.LargeImage = btnLintelsSectionsImage;
            btnLintelsSections.ToolTip =
                "Создание разрезов по каждому уникальному сечению перемычки " +
                "для последующего формирования ведомости перемычек.";
            btnLintelsSections.LongDescription = "Уникальность сечения определяется как уникальный набор значений " +
                "толщины стены, наименований вложенных семейств перемычки, " +
                "расстояния между двумя крайними продольными элементами перемычки," +
                "и наименования уровня (опционально, если подсчет перемычек поэтажный). " +
                "Перемычки должны быть созданы категорией \'Обобщенные модели\', " +
                "в Описании типоразмера родительского семейства перемычки (и только в нем!) " +
                "должно быть написано \'Перемычка\'." +
                "\nОформлять разрезы необходимо вручную, но границу обрезки изменять нежелательно, " +
                "т.к. впоследствии при формировании изображений у них будет отличаться масштаб.";

            // CreateImagesFromSections command
            PushButtonData btnCreateImagesFromSections = new PushButtonData("CreateImagesFromSections", "Ведомость\nперемычек", path, "MS.Commands.AR.CreateImagesFromSections");
            Uri btnCreateImagesFromSectionsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnCreateImagesFromSectionsImage = new BitmapImage(btnCreateImagesFromSectionsImagePath);
            btnCreateImagesFromSections.LargeImage = btnCreateImagesFromSectionsImage;
            btnCreateImagesFromSections.ToolTip = "Формирование изображений из разрезов " +
                "(необходимо их оформить вручную, не изменяя границу обрезки, " +
                "т.к. при этом изменится масштаб изображения) по перемычкам, " +
                "сформированным командой \'Разрезы по перемычкам\' и их назначение на перемычкам в параметр " +
                "\'PGS_ИзображениеТипоразмераМатериала\' для ведомости перемычек. Также перемычкам " +
                "назначается \'PGS_МногострочнаяМарка\', в которой написаны все марки перемычек " +
                "с одинаковым поперечным сечением.";

            // RoomsFinishingMultiMark command
            PushButtonData btnRoomsFinishingMultiMark = new PushButtonData("RoomsFinishingMultiMark", "Помещения\nс одинаковой отделкой", path, "MS.Commands.AR.RoomsFinishingMultiMark");
            Uri btnRoomsFinishingMultiMarkImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnRoomsFinishingMultiMarkImage = new BitmapImage(btnRoomsFinishingMultiMarkImagePath);
            btnRoomsFinishingMultiMark.LargeImage = btnRoomsFinishingMultiMarkImage;
            btnRoomsFinishingMultiMark.ToolTip = "Назначение \'PGS_МногострочнаяМарка\' " +
                "для помещений с одинаковой отделкой стен и потолка и \'PGS_МногострочнаяМарка_2\' " +
                "для помещений с одинаковой отделкой пола.";
            btnRoomsFinishingMultiMark.LongDescription =
                "Отделка помещений должна быть выполнена параметрами и задана через ключевую спецификацию. " +
                "Параметры для многострочных марок не должны быть в этих ключевых спецификациях.";


            // RoomsFinishingCommand
            //PushButtonData btnRmFinishing = new PushButtonData("RoomsFinishing\nDebug", "Отделка\n(Beta)", path, "MS.Utilites.GetBoundarySegmentElement");
            //Uri btnRmFinishingImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\RoomsFinishing.png");
            //BitmapImage btnRmFinishingImage = new BitmapImage(btnRmFinishingImagePath);
            //btnRmFinishing.LargeImage = btnRmFinishingImage;


            // MaterialColorsCommand
            PushButtonData btnMaterialColors = new PushButtonData("MaterialColors", "Обновить\nцвета", path, "MS.Commands.AR.MaterialColors");
            Uri btnMaterialColorsImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\MaterialColors.png");
            BitmapImage btnMaterialColorsImage = new BitmapImage(btnMaterialColorsImagePath);
            btnMaterialColors.LargeImage = btnMaterialColorsImage;
            btnMaterialColors.ToolTip = "Формирование цветового изображения по RGB материала " +
                "и назначение в параметр \'PGS_ИзображениеТипоразмераМатериала\' " +
                "для формирования ведомости отделки фасада.";
            btnMaterialColors.LongDescription = "Обрабатываются все без исключения материалы. " +
                "Если RGB материала изменился, изображение будет обновлено. " +
                "Лишние изображения из проекта не удаляются.";


            // StairReinforcement command
            //PushButtonData stairRnfrcmtCmd = new PushButtonData("StairReinforcement", "Армирование\nлестниц", path, "MS.Commands.KR.StairReinforcement");
            //Uri stairRnfrcmtCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\KR\StairRnfrcmt.png");
            //BitmapImage stairRnfrcmtCmdImage = new BitmapImage(stairRnfrcmtCmdImagePath);
            //stairRnfrcmtCmd.LargeImage = stairRnfrcmtCmdImage;

            //Диапазон марок свай command
            PushButtonData btnPilesMarkRangeCmd = new PushButtonData(
                "PilesMarkRange",
                "Маркировка\nсвай",
                path,
                "MS.Commands.KR.PilesMarkRange");
            Uri btnPilesMarkRangeCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnPilesMarkRangeCmdImage = new BitmapImage(btnPilesMarkRangeCmdImagePath);
            btnPilesMarkRangeCmd.LargeImage = btnPilesMarkRangeCmdImage;
            btnPilesMarkRangeCmd.ToolTip = "Назначение диапазона Марок выбранным сваям (несущим колоннам) " +
                "с одинаковой Мрк.МаркаКонструкции.";
            btnPilesMarkRangeCmd.LongDescription = "Для маркировки у категории \'Несущие колонны\'" +
                " должны быть общие параметры \'Мрк.МаркаКонструкции\' и \'Орг.Диапазон позиций\'";
            btnPilesMarkRangeCmd.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, @"https://google.com"));


            // SS Numerator command
            //PushButtonData btnNumerator = new PushButtonData("Numerator", "Маркировка", path, "MS.Numerator");
            //Uri btnNumeratorImagePath = new Uri(assembly_dir + @"\Images\Icons\SS\СС.png");
            //BitmapImage btnNumeratorImage = new BitmapImage(btnNumeratorImagePath);
            //btnNumerator.LargeImage = btnNumeratorImage;

            // ОВиК арматура трубопроводов и оборудование в помещениях command
            PushButtonData btnPipelineFittings = new PushButtonData("PipelineFittings", "Арматура\nтрубопроводов", path, "MS.Commands.MEP.PipelineFittings");
            Uri btnPipelineFittingsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnPipelineFittingsImage = new BitmapImage(btnPipelineFittingsImagePath);
            btnPipelineFittings.LargeImage = btnPipelineFittingsImage;
            btnPipelineFittings.ToolTip = "Автоматическое заполнение пунктов номера квартиры " +
                "в спецификации \"PGS_ОВ_Настройка поквартирных клапанов\".";
            btnPipelineFittings.LongDescription = "Копирование значения прааметров в экземпляры " +
                "элементов категорий: Арматура трубопроводов и в Оборудование " +
                "пишется значение номер квартиры из помещения, в которых эти экземпляры расположены.";
            btnPipelineFittings.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, @"https://google.com"));


            // ОВиК системы в пространствах command
            PushButtonData btnSystemsInSpace = new PushButtonData("SystemsInSpace", "Системы\nв пространствах", path, "MS.Commands.MEP.SystemsInSpace");
            Uri btnSystemsInSpaceImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnSystemsInSpaceImage = new BitmapImage(btnSystemsInSpaceImagePath);
            btnSystemsInSpace.LargeImage = btnSystemsInSpaceImage;
            btnSystemsInSpace.ToolTip = "Автоматическое заполнение пунктов принадлежности " +
                "систем притока и вытяжки для таблицы воздухообменов.";
            btnSystemsInSpace.LongDescription = "Копирование значений параметров воздуховодов " +
                "\"Имя системы\" для притока и вытяжки в параметры пространства " +
                "\"ADSK_Наименование приточной системы\" и \"ADSK_Наименование вытяжной системы\". " +
                "Для самостоятельного заполнения пункта в параметр пространства \"Комментарии\" " +
                "написать \"не обрабатывать\" (регистр не важен)";
            btnSystemsInSpace.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, @"https://google.com"));


            // General panel
            panelGeneral.AddItem(btnInfo);
            //panelGeneral.AddItem(btnLevelName);

            // AR panel
            panelAR.AddItem(btnRmArea);
            panelAR.AddItem(btnCmplxAprtNum);
            panelAR.AddItem(btnOpeningsArea);
            panelAR.AddItem(btnMarkLintelsInOpenings);
            panelAR.AddItem(btnLintelsSections);
            panelAR.AddItem(btnCreateImagesFromSections);
            panelAR.AddItem(btnRoomsFinishingMultiMark);
            //panelAR.AddItem(btnRmFinishing); // в разработке, нужно указать названия параметров
            panelAR.AddItem(btnSelection);
            panelAR.AddItem(btnMasonryMesh);
            panelAR.AddItem(btnMaterialColors);

            // KR panel
            //panelKR.AddItem(stairRnfrcmtCmd); // в разработке (сырая)
            panelKR.AddItem(btnPilesMarkRangeCmd);

            // SS panel
            // panelSS.AddItem(btnNumerator);

            // ОВиК panel
            panelOVVK.AddItem(btnPipelineFittings);
            panelOVVK.AddItem(btnSystemsInSpace);
        }
    }
}
