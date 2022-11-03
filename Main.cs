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
            RibbonPanel panelOV = application.CreateRibbonPanel("PGS-BIM", "Раздел ОВ");
            RibbonPanel panelVK = application.CreateRibbonPanel("PGS-BIM", "Раздел ВК");
            RibbonPanel panelBIM = application.CreateRibbonPanel("PGS-BIM", "BIM");


            // Info command
            PushButtonData btnInfo = new PushButtonData("Info", "Alt\nсимволы", path, "MS.Info");
            Uri btnInfoImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Alt.png");
            BitmapImage btnInfoImage = new BitmapImage(btnInfoImagePath);
            btnInfo.LargeImage = btnInfoImage;
            btnInfo.ToolTip = "Alt-символы Revit.";

            // ClashReport command
            PushButtonData btnClashReport = new PushButtonData("ClashReport", "Clashes\nimport", path, "MS.Commands.BIM.ClashReportImport");
            Uri btnClashReportImagePath = new Uri(assembly_dir + @"\Images\Icons\BIM\Clashes.png");
            BitmapImage btnClashReportImage = new BitmapImage(btnClashReportImagePath);
            btnClashReport.LargeImage = btnClashReportImage;
            btnClashReport.ToolTip = "Расстановка семейств по координатом пересечений из отчета Navisworks.";
            btnClashReport.LongDescription = $"\n\nНазвание проверки записано в ADSK_Группирование" +
                $"\nОтветственный записывается в 'Комментарии'" +
                $"\nid1 записывается в 'ADSK_Код изделия'" +
                $"\nid2 записывается в 'ADSK_Позиция'" +
                $"\nname1 записывается в 'ADSK_Наименование'" +
                $"\nname2 записывается в 'ADSK_Обозначение'" +
                $"\nclashresult записывается в 'ADSK_Примечание'";

            // PolyLineLength command
            PushButtonData btnPolyLineLength = new PushButtonData("PolyLineLength", "Длина\nполилинии", path, "MS.Commands.General.PolyLineLength");
            Uri btnPolyLineLengthImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Sigma.png");
            BitmapImage btnPolyLineLengthImage = new BitmapImage(btnPolyLineLengthImagePath);
            btnPolyLineLength.LargeImage = btnPolyLineLengthImage;
            btnPolyLineLength.ToolTip = "Суммарная длина выбранных линий";
            btnPolyLineLength.LongDescription = "Выберите линии до нажатия на кнопку " +
                "или нажмите кнопку и выберите линии рамкой или кликом, поcле чего нажмите 'Готово'. " +
                "\nМожно выбрать как линии детализации, так и линии модели. " +
                "\nЕсли до нажатия команды выбраны элементы " +
                "разных категорий, то линии из них отфильтруются автоматически.";


            // LevelName command
            //PushButtonData btnLevelName = new PushButtonData("LevelName", "Отметки\nуровней", path, "MS.Commands.General.LevelName");
            //Uri btnLevelNameImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            //BitmapImage btnLevelNameImage = new BitmapImage(btnLevelNameImagePath);
            //btnLevelName.LargeImage = btnLevelNameImage;
            //btnLevelName.ToolTip = "Проверка отметок уровней в их названии";


            // Selection command
            PushButtonData btnSelection = new PushButtonData("Selection", "Выбор\nэлементов", path, "MS.Selector");
            Uri btnSelectionImagePath = new Uri(assembly_dir + @"\Images\Icons\General\SelectionElements.png");
            BitmapImage btnSelectionImage = new BitmapImage(btnSelectionImagePath);
            btnSelection.LargeImage = btnSelectionImage;
            btnSelection.ToolTip =
                "Интерфейс переключается в режим выбора только элементов заданной категории.";
            btnSelection.LongDescription =
                "Категория задается при первом запуске во всплывающем окне " +
                "и запоминается для последующего использования в сеансе Revit. " +
                "В последствии можно изменить категорию в настройках, нажав на треуголник под панелью General.";

            // SelectionSettings command
            PushButtonData btnSelectionSettings = new PushButtonData("SelectionSettings", "Настройка\nкатегории", path, "MS.Commands.General.SelectorSettingsCmd");
            Uri btnSelectionSettingsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Settings.png");
            BitmapImage btnSelectionSettingsImage = new BitmapImage(btnSelectionSettingsImagePath);
            btnSelectionSettings.LargeImage = btnSelectionSettingsImage;
            btnSelectionSettings.ToolTip =
                "Настройка категории для выбора элементов.";
            btnSelectionSettings.LongDescription =
                "Категория задается во всплывающем окне и запоминается для последующего использования в сеансе Revit. " +
                "Доступны все категории Revit, даже группы.";

            // MasonryMesh command
            PushButtonData btnMasonryMesh = new PushButtonData("MasonryMesh", "Кладочная\nсетка", path, "MS.Commands.AR.MasonryMesh");
            Uri btnMasonryMeshImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\MasonryMesh.png");
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
            Uri btnCmplxAprtNumImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\Bricks.png");
            BitmapImage btnCmplxAprtNumImage = new BitmapImage(btnCmplxAprtNumImagePath);
            btnCmplxAprtNum.LargeImage = btnCmplxAprtNumImage;
            btnCmplxAprtNum.ToolTip = "Заполнение составного номера квартиры в Комментирии";
            btnCmplxAprtNum.LongDescription = "Составной номер будет компоноваться из значений параметров " +
                "\'ADKS_Количество комнат\', \'ADSK_Тип квартиры\' и \'ADSK_Индекс квартиры\'." +
                " Полученное значение будет записано в параметр \'Комментарии\'";

            // OpeningsByDuct command
            PushButtonData btnOpeningsByDuctCmd = new PushButtonData("OpeningByDuct", "Проем по\nвоздуховоду", path, "MS.Commands.AR.OpeningByDuctCmd");
            Uri btnOpeningsByDuctCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnOpeningsByDuctCmdImage = new BitmapImage(btnOpeningsByDuctCmdImagePath);
            btnOpeningsByDuctCmd.LargeImage = btnOpeningsByDuctCmdImage;
            btnOpeningsByDuctCmd.ToolTip = "Проем по воздуховоду";
            btnOpeningsByDuctCmd.LongDescription = "";

            // OpeningsArea command
            PushButtonData btnOpeningsArea = new PushButtonData("OpeningsArea", "Площадь проемов\nв помещениях", path, "MS.Commands.AR.OpeningsArea");
            Uri btnOpeningsAreaImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\BalconyDoor.png");
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
            Uri btnMarkLintelsInOpeningsImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\Beams.png");
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
            Uri btnLintelsSectionsImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\Section.png");
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
            Uri btnCreateImagesFromSectionsImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\TableLintels.png");
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
            Uri btnRoomsFinishingMultiMarkImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\Equal.png");
            BitmapImage btnRoomsFinishingMultiMarkImage = new BitmapImage(btnRoomsFinishingMultiMarkImagePath);
            btnRoomsFinishingMultiMark.LargeImage = btnRoomsFinishingMultiMarkImage;
            btnRoomsFinishingMultiMark.ToolTip = "Назначение \'PGS_МногострочнаяМарка\' " +
                "для помещений с одинаковой отделкой стен и потолка и \'PGS_МногострочнаяМарка_2\' " +
                "для помещений с одинаковой отделкой пола.";
            btnRoomsFinishingMultiMark.LongDescription =
                "Отделка помещений должна быть выполнена параметрами и задана через ключевую спецификацию. " +
                "Параметры для многострочных марок не должны быть в этих ключевых спецификациях.";

            // RoomsFinCreationCommand
            PushButtonData btnRmFinCreation = new PushButtonData("RoomsFinishing", "Отделка\nпомещений", path, "MS.Commands.AR.RoomsFinishingCreation");
            Uri btnRmFinCreationImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\RoomsFinishing.png");
            BitmapImage btnRmFinCreationImage = new BitmapImage(btnRmFinCreationImagePath);
            btnRmFinCreation.LargeImage = btnRmFinCreationImage;
            btnRmFinCreation.ToolTip = "Создание отделочных стен внутри выбранных помещений." +
                "\nПомещения можно выбрать в спецификации и запустить команду, или запустить команду и выбрать их вручную.";
            btnRmFinCreation.LongDescription = "По границам помещения определяются элементы, которые ее образуют и, " +
                "если это элемент категории 'Стены' или 'Несущие колонны', " +
                "то строится соединенная с этим элементом отделочная стена. " +
                "Типоразмеры отделочных стен определяются по значению параметра " +
                "'PGS_НаименованиеОтделки' в типе отделываемого элемента " +
                "и назначаюстя через всплывающее окно. Если типоразмер не назначен для данного значения параметра, " +
                "то по этому элементу отделка создаваться не будет." +
                "\nДля связей и типоразмеров элементов " +
                "с пустым или не назначенным параметром 'PGS_НаименованиеОтделки'" +
                "во всплывающем окне будет сформирована строчка 'НЕ НАЗНАЧЕНО'.";


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

            // RoomBoardsFinishingCommand
            PushButtonData btnRoomBoardsFinishing = new PushButtonData("RoomBoardsFinishing", "Откосы\nи плинтусы\n(Beta)", path, "MS.Commands.AR.RoomBoardsFinishingCommand");
            Uri btnRoomBoardsFinishingImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\Plinth.png");
            BitmapImage btnRoomBoardsFinishingImage = new BitmapImage(btnRoomBoardsFinishingImagePath);
            btnRoomBoardsFinishing.LargeImage = btnRoomBoardsFinishingImage;
            btnRoomBoardsFinishing.ToolTip = "Подсчет площадей откосов и длины плинтуса в выбранных помещениях";
            btnRoomBoardsFinishing.LongDescription = "Во всех выбранных помещениях с ненулевой площадью " +
                "будут найдены витражи, " +
                "у которых в экземпляре параметр 'PGS_Глубина откосов' > 0. " +
                "По пересечению их контуров (кроме нижней горизонтальной линии подоконника) с помещением " +
                "будет посчитан периметр откосов и их площадь." +
                "\nТакже будет посчитана длина плинтуса в помещении как разница длины периметра помещения " +
                "и суммы сегментов границ помещения, образованных линией разделителей помещений " +
                "и ширины всех дверей в помещении." +
                "\nВАЖНО! Для корректной обработки дверей, сделанных витражами необходимо построить " +
                "линию разделения помещений по границе помещения возле этих витражей." +
                "\nЗначения будут записаны в параметры Помещения 'PGS_Откосы_Площадь' и 'PGS_Длина_Плинтус'." +
                "\n\nПомещения можно сначала выбрать и запустить команду " +
                "(даже если выбраны не только помещения, то они автоматически отфильтруются) " +
                "или запустить команду и выбрать необходимые на виде помещения.";

            // RoomFinishingScheduleCreationCmd
            PushButtonData btnRoomFinishingScheduleCreationCmd = new PushButtonData("RoomFinishingScheduleCreationCmd", "Ведомость\nотделки (beta)", path, "MS.Commands.AR.RoomFinishingScheduleCreationCmd");
            Uri btnRoomFinishingScheduleCreationCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\TableFinishing.png");
            BitmapImage btnRoomFinishingScheduleCreationCmdImage = new BitmapImage(btnRoomFinishingScheduleCreationCmdImagePath);
            btnRoomFinishingScheduleCreationCmd.LargeImage = btnRoomFinishingScheduleCreationCmdImage;
            btnRoomFinishingScheduleCreationCmd.ToolTip = "Создание спецификации ведомости отделки " +
                "по объемам отделки выбранных помещений";
            btnRoomFinishingScheduleCreationCmd.LongDescription = "Необходимо создать отделочные стены в помещениях, " +
                "в названиях типоразмеров которых содержится '_F_' и создать потолки. Значение параметра типа 'Описание' " +
                "у стен и потолков будет записываться в соответствующие ячейки сгенерированной ведомости отделки." +
                "\nПомещения можно выбрать в спецификации и запустить команду, или запустить команду и выбрать их вручную.";


            //StairReinforcement command
            PushButtonData stairRnfrcmtCmd = new PushButtonData("StairReinforcement", "Армирование\nлестниц", path, "MS.Commands.KR.StairReinforcement");
            Uri stairRnfrcmtCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\KR\StairRnfrcmt.png");
            BitmapImage stairRnfrcmtCmdImage = new BitmapImage(stairRnfrcmtCmdImagePath);
            stairRnfrcmtCmd.LargeImage = stairRnfrcmtCmdImage;

            //Диапазон марок свай command
            PushButtonData btnPilesMarkRangeCmd = new PushButtonData(
                "PilesMarkRange",
                "Маркировка\nсвай",
                path,
                "MS.Commands.KR.PilesMarkRange");
            Uri btnPilesMarkRangeCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\KR\PilesMachine.png");
            BitmapImage btnPilesMarkRangeCmdImage = new BitmapImage(btnPilesMarkRangeCmdImagePath);
            btnPilesMarkRangeCmd.LargeImage = btnPilesMarkRangeCmdImage;
            btnPilesMarkRangeCmd.ToolTip = "Назначение диапазона Марок выбранным сваям (несущим колоннам) " +
                "с одинаковой Мрк.МаркаКонструкции.";
            btnPilesMarkRangeCmd.LongDescription = "Для маркировки у категории \'Несущие колонны\'" +
                " должны быть общие параметры \'Мрк.МаркаКонструкции\' и \'Орг.Диапазон позиций\'";
            btnPilesMarkRangeCmd.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, @"https://google.com"));

            //Проемы по инженерке
            PushButtonData btnOpeningByMEPCmd = new PushButtonData(
                "OpeningByMEP",
                "Проемы\nпо инженерке",
                path,
                "MS.Commands.KR.OpeningByMEPCmd");
            Uri btnOpeningByMEPCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnOpeningByMEPCmdImage = new BitmapImage(btnOpeningByMEPCmdImagePath);
            btnOpeningByMEPCmd.LargeImage = btnOpeningByMEPCmdImage;
            btnOpeningByMEPCmd.ToolTip = "Размещение проемов по воздуховодам и трубам";
            btnOpeningByMEPCmd.LongDescription = "В месте пересечения выделенной трубы/воздуховода и стены/плиты" +
                " размещается заданный типоразмер семейства проема с заданным отступом";
            btnOpeningByMEPCmd.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, @"https://google.com"));

            //Проемы по инженерке настройка
            PushButtonData btnChangeSettingsKRCmd = new PushButtonData(
                "SettingsKR",
                "Настройки",
                path,
                "MS.Commands.KR.ChangeSettingsCmd");
            Uri btnChangeSettingsKRCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnChangeSettingsKRCmdImage = new BitmapImage(btnChangeSettingsKRCmdImagePath);
            btnChangeSettingsKRCmd.LargeImage = btnChangeSettingsKRCmdImage;
            btnChangeSettingsKRCmd.ToolTip = "Настройка размещения проемов по воздуховодам и трубам";
            btnChangeSettingsKRCmd.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, @"https://google.com"));


            // SS Numerator command
            //PushButtonData btnNumerator = new PushButtonData("Numerator", "Маркировка", path, "MS.Numerator");
            //Uri btnNumeratorImagePath = new Uri(assembly_dir + @"\Images\Icons\SS\СС.png");
            //BitmapImage btnNumeratorImage = new BitmapImage(btnNumeratorImagePath);
            //btnNumerator.LargeImage = btnNumeratorImage;

            // ОВиК арматура трубопроводов и оборудование в помещениях command
            PushButtonData btnPipelineFittings = new PushButtonData("PipelineFittings", "Инженерные элементы\nв пространствах", path, "MS.Commands.MEP.PipelineFittings");
            Uri btnPipelineFittingsImagePath = new Uri(assembly_dir + @"\Images\Icons\MEP\WaterPump.png");
            BitmapImage btnPipelineFittingsImage = new BitmapImage(btnPipelineFittingsImagePath);
            btnPipelineFittings.LargeImage = btnPipelineFittingsImage;
            btnPipelineFittings.ToolTip = "Автоматическое заполнение пунктов номера квартиры " +
                "в спецификации \"PGS_ОВ_Настройка поквартирных клапанов\".";
            btnPipelineFittings.LongDescription = "Копирование значения параметра пространства (или помещения) " +
                "в экземпляры элементов категорий: Арматура трубопроводов, Оборудование, Воздухораспределители на выбор. " +
                "Пишется значение 'ADSK_Номер квартиры' из помещения (или 'Номер помещения' из пространства), " +
                "в расположенные в них экземпляры элементов выбранных категорий." +
                "\n\nВажно! Перед запуском команды убедитесь, что геометрия пространств (помещений) доходит до потолка, " +
                "иначе не будет найдена принадлежность элемента инженерных систем данному пространству (помещению).";
            btnPipelineFittings.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, @"https://google.com"));


            // ОВиК системы в пространствах command
            PushButtonData btnSystemsInSpace = new PushButtonData("SystemsInSpace", "Системы\nв пространствах", path, "MS.Commands.MEP.SystemsInSpace");
            Uri btnSystemsInSpaceImagePath = new Uri(assembly_dir + @"\Images\Icons\MEP\Ventilation.png");
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

            // ОВиК корректировка имени системы command
            PushButtonData btnSystemNameCorrectCmd = new PushButtonData("SystemNameCorrectCmd", "Скорректировать\nИмяСистемы", path, "MS.Commands.MEP.SystemNameCorrectCmd");
            Uri btnSystemNameCorrectCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\MEP\UpdateTable.png");
            BitmapImage btnSystemNameCorrectCmdImage = new BitmapImage(btnSystemNameCorrectCmdImagePath);
            btnSystemNameCorrectCmd.LargeImage = btnSystemNameCorrectCmdImage;
            btnSystemNameCorrectCmd.ToolTip = "Корректировка параметра 'ИмяСистемы' у элементов категорий " +
                "'Оборудование', " +
                "'Сантехническоие приборы', " +
                "'Арматура трубопроводов', " +
                "'Гибкие трубы', " +
                "'Соединительные детали трубопроводов', " +
                "'Трубы'." +
                "По таблице Excel задается буква системы, которую нужно оставить у экземпляра семейства.";
            btnSystemNameCorrectCmd.LongDescription = "Из первого столбца, начиная с первой строки, " +
                "берется строковое значение, которое должно содержаться в параметре ADSK_Наименование " +
                "у элементов обрабатываемых категорий с несколькими системами. " +
                "Во втором столбце напротив каждого строкового значения " +
                "должна быть буква, обозначающая систему, которую необходимо оставить, например 'В'. " +
                "Если у какого-то элемента несколько систем одного и того же типа, или нет системы с заданной буквой, " +
                "а также если не задан параметр ADSK_Наименование, будет выведено сообщение с Id этого элемента." +
                "\nПравила составления Excel файла:" +
                "\n1. Заполнять таблицу в первых двух столбцах, начиная с первой строки (шапку не использовать)." +
                "\n2. В таблице не должно быть пустых строчек, которые разделяют строки со значениями." +
                "\n3. В первом столбце не должно быть повторяющихся значений." +
                "\n4. Оформление таблицы не влияет на работу команды.";

            // ОВиК корректировка имени системы command
            PushButtonData btnEquipmentInSystemCmd = new PushButtonData("EquipmentInSystemCmd", "Оборудование\nв системах", path, "MS.Commands.MEP.EquipmentInSystemCmd");
            Uri btnEquipmentInSystemCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\MEP\SystemEquipment.png");
            BitmapImage btnEquipmentInSystemCmdImage = new BitmapImage(btnEquipmentInSystemCmdImagePath);
            btnEquipmentInSystemCmd.LargeImage = btnEquipmentInSystemCmdImage;
            btnEquipmentInSystemCmd.ToolTip = "Переписывает характеристики и количество воздухонагревателей, воздухоохладителей и фильтров в параметры вентиляторов в системах";
            btnEquipmentInSystemCmd.LongDescription = "Необходимые общие параметры для воздухонагревателей " +
                "(могут быть добавлены в тип и в экземпляр):" +
                "\nPGS_Идентификация = 2" +
                "\nADSK_Потеря давления воздуха в нагревателе" +
                "\nADSK_Температура воздуха на входе в нагреватель" +
                "\nADSK_Температура воздуха на выходе из нагревателя" +
                "\nADSK_Тепловая мощность" +
                "\nPGS_ВоздухонагревательМощность" +
                "\nPGS_ВоздухонагревательТип" +
                "\n\nНеобходимые общие параметры для воздухоохлвдителей:" +
                "\nPGS_Идентификация = 3" +
                "\nADSK_Потеря давления воздуха в охладителе" +
                "\nADSK_Температура воздуха на входе в охладитель" +
                "\nADSK_Температура воздуха на выходе из охладителя" +
                "\nADSK_Холодильная мощность" +
                "\nPGS_ВоздухоохладительМощность" +
                "\nPGS_ВоздухоохладительТип" +
                "\n\nНеобходимые общие параметры для фильтров:" +
                "\nPGS_Идентификация = 4" +
                "\nADSK_Сопротивление воздушного фильтра" +
                "\nPGS_ФильтрТип" +
                "\n\nНеобходимые общие параметры для вентиляторов" +
                "\nPGS_Идентификация = 1" +
                "\nPGS_ВоздухонагревательКоличество" +
                "\nPGS_ВоздухоохладительКоличество" +
                "\nPGS_ФильтрКоличество" +
                "\nи ВСЕ параметры для воздухонагревателей, воздухоохладителей и фильтров.";


            // ОВиК корректировка имени системы command
            PushButtonData btnFullSystemNamesCmd = new PushButtonData("FullSystemNamesCmd", "Полные имена\nсистем", path, "MS.Commands.MEP.FullSystemNamesCmd");
            Uri btnFullSystemNamesCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\MEP\Find.png");
            BitmapImage btnFullSystemNamesCmdImage = new BitmapImage(btnFullSystemNamesCmdImagePath);
            btnFullSystemNamesCmd.LargeImage = btnFullSystemNamesCmdImage;
            btnFullSystemNamesCmd.ToolTip = "Записывает полное имя системы по значению параметра 'ИмяСистемы' " +
                "в соответствии с заданным значением в Excel.";
            btnFullSystemNamesCmd.LongDescription = "\nПравила составления Excel файла:" +
                "\n1. Заполнять таблицу в первых двух столбцах, начиная с первой строки (шапку не использовать)." +
                "\n2. В таблице не должно быть пустых строчек, которые разделяют строки со значениями." +
                "\n3. В первом столбце не должно быть повторяющихся значений." +
                "\n4. Регистр имеет значение." +
                "\n5. Оформление таблицы не влияет на работу команды.";

            // General panel
            panelGeneral.AddItem(btnInfo);
            panelGeneral.AddItem(btnPolyLineLength);
            panelGeneral.AddItem(btnSelection);
            panelGeneral.AddSlideOut();
            panelGeneral.AddItem(btnSelectionSettings);

            //panelGeneral.AddItem(btnLevelName);

            // AR panel
            panelAR.AddItem(btnRmArea);
            panelAR.AddItem(btnCmplxAprtNum);
            panelAR.AddItem(btnOpeningsArea);
            panelAR.AddItem(btnMarkLintelsInOpenings);
            panelAR.AddItem(btnLintelsSections);
            panelAR.AddItem(btnCreateImagesFromSections);
            panelAR.AddItem(btnRoomsFinishingMultiMark);
            panelAR.AddItem(btnRmFinCreation);
            panelAR.AddItem(btnRoomFinishingScheduleCreationCmd);
            panelAR.AddItem(btnRoomBoardsFinishing);
            panelAR.AddItem(btnMasonryMesh);
            panelAR.AddItem(btnMaterialColors);
            panelAR.AddSlideOut();
            panelAR.AddItem(btnOpeningsByDuctCmd);

            // KR panel
            panelKR.AddItem(stairRnfrcmtCmd); // в разработке (сырая)
            panelKR.AddItem(btnPilesMarkRangeCmd);
            panelKR.AddSlideOut();
            panelKR.AddItem(btnOpeningByMEPCmd);
            panelKR.AddItem(btnChangeSettingsKRCmd);

            // SS panel
            // panelSS.AddItem(btnNumerator);

            // ОВ panel
            panelOV.AddItem(btnPipelineFittings);
            panelOV.AddItem(btnSystemsInSpace);
            panelOV.AddItem(btnEquipmentInSystemCmd);

            // ВК panel
            panelVK.AddItem(btnSystemNameCorrectCmd);
            panelVK.AddItem(btnFullSystemNamesCmd);

            // BIM panel
            panelBIM.AddItem(btnClashReport);
        }
    }
}
