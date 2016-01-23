﻿using System.Collections.ObjectModel;

namespace PageReplacer.Models
{
    public class PageReplacerLastFrequentlyUsed : IPageReplacerEvent
    {
        public PageReplacerLastFrequentlyUsed(PR_SimulatorModel simulatorModel, ObservableCollection<string> parameters)
        {
            this.simulatorModel = simulatorModel;
            
        }

        //private int freezeTime;

        PR_SimulatorModel simulatorModel;
        public PR_SimulatorModel SimulatorModel
        {
            get { return simulatorModel; }
            set { simulatorModel = value; }
        }

        public PageRecord processPageRequestAndReturnNewPageRecordAccordingToHistory(int i, PageRecord pageRecord)
        {
            if (pageRecord != null)
            {
                // csak a legelső iterációban lehet üres, amikor még a History is üres
                if (pageRecord.Pages.Count == 0)
                {
                    // ha tényleg nincs benne semmi, vissza kell adni legalább az igény és annak behozatalából összeállított rekordot
                    PageRecord clone = MyCloner.DeepClone<PageRecord>(pageRecord.CreateNewPageByPageNumberAndShiftTheOtherPages(i));
                    clone.setTimestampOnPage(i, simulatorModel.StepCounter);
                    return clone;
                }

                // ha már tartalmazza a kívánt lapot, akkor nem kell kivinni senkit
                if (pageRecord.containsPage(i))
                {
                    PageRecord clone = MyCloner.DeepClone<PageRecord>(pageRecord);
                    clone.setReferenceAndPageFault(i, false);
                    clone.setTimestampOnPage(i, simulatorModel.StepCounter);
                    return clone;
                }

                // ha nem tartalmazza a kívánt lapot az új lapot be kell vinni

                // ha van elég hely a tárban, a lapot beszúrjuk az első helyre
                if (pageRecord.hasMoreSpaceForNewPage())
                {
                    pageRecord.Pages.Insert(0, new Page(i));
                    pageRecord.setReferenceAndPageFault(i, true);
                    pageRecord.setTimestampOnPage(i, simulatorModel.StepCounter);
                    return pageRecord;
                }

                // ha nincs elég hely a tárban, áldozatot kell kiválasztani
                Page actualPage = null;
                foreach (Page page in pageRecord.Pages)
                {
                    if (actualPage == null)
                    {
                        actualPage = page;
                        continue;
                    }
                    if (actualPage.LfuCounter > page.LfuCounter)
                    {
                        actualPage = page;
                    }
                }

                // az áldozat helyére
                if (actualPage != null)
                {
                    pageRecord.ReplaceExistingPageWithNewPage(actualPage, i);
                    pageRecord.setTimestampOnPage(i, simulatorModel.StepCounter);
                    return pageRecord;
                }

            }
            return null;
        }

        public bool usesPeriodsToRemoveRbits()
        {
            return true;
        }


        public PageRecord reactOnPeriodicalRbitRemovalEvent(PageRecord oldPageRecord)
        {
            foreach (Page page in oldPageRecord.Pages)
            {
                if (page.Rbit == false)
                {
                    page.LfuCounter += 0;
                    continue;
                }
                if (page.Rbit == true)
                {
                    page.LfuCounter++;
                    page.Rbit = false;
                }
            }
            return MyCloner.DeepClone<PageRecord>(oldPageRecord);            
        }
    }
}
