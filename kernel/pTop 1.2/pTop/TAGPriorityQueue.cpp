/*
 *  TAGPriorityQueue.h
 *
 *  Created on: 2014-04-08
 *  Author: luolan
 */
#include <iostream>
#include <cstring>
#include <vector>
#include <string>
#include <algorithm>
#include "TagFlow.h"
#include "TAGPriorityQueue.h"

using namespace std;

CPriorityQueue::CPriorityQueue(const int maxsize)
{
	m_nMaxSize = maxsize;
	m_qTags.clear();
}

CPriorityQueue::~CPriorityQueue()
{
	m_nMaxSize = 0;
	if(m_qTags.size() != 0)
	{
		m_qTags.clear();
	}
}

void CPriorityQueue::_AdjustQueue(int tPos)
{
	while (1)
    {
        int tPosLeftChild = (tPos << 1) + 1;
		if (tPosLeftChild < m_nMaxSize && m_qTags[tPos].lfScore > m_qTags[tPosLeftChild].lfScore)
        { // lfScore of the tPos node is greater than its left child
            if (tPosLeftChild + 1 < m_nMaxSize && m_qTags[tPos].lfScore > m_qTags[tPosLeftChild + 1].lfScore)
            { // lfScore of the tPos node is greater than its right child
                if (m_qTags[tPosLeftChild].lfScore < m_qTags[tPosLeftChild + 1].lfScore)
                { // The lfScore of the left child is smaller
                        swap(m_qTags[tPos], m_qTags[tPosLeftChild]);
                        tPos = tPosLeftChild;
                } else { // The lfScore of the right child is smaller
                     swap(m_qTags[tPos], m_qTags[tPosLeftChild + 1]);
                     tPos = tPosLeftChild + 1;
                }
            } else { // lfScore of the tPos node is no greater than its right child
                swap(m_qTags[tPos], m_qTags[tPosLeftChild]);
                tPos = tPosLeftChild;
            }
        } else { // lfScore of the tPos node is no greater than its left child
             if (tPosLeftChild + 1 < m_nMaxSize && m_qTags[tPos].lfScore > m_qTags[tPosLeftChild + 1].lfScore)
             {
                   swap(m_qTags[tPos], m_qTags[tPosLeftChild + 1]);
                   tPos = tPosLeftChild + 1;
             } else break;
        }
    } // end while
}

bool CPriorityQueue::_IsExistinQueue(const TAGRES &addTag)
{
	vector<TAGRES>::iterator it;
	int idx = 0;
	for(it = m_qTags.begin(); it != m_qTags.end(); ++it)
	{
		if( it->strTag.compare(addTag.strTag) == 0 && 
			fabs(it->lfFlankingMass[0] - addTag.lfFlankingMass[0]) < 0.000001 &&
			fabs(it->lfFlankingMass[1] - addTag.lfFlankingMass[1]) < 0.000001 &&
			it->vmodID == addTag.vmodID )
		{ // TO CHECK
			if(it->lfScore < addTag.lfScore)
			{
				it->lfScore = addTag.lfScore;
				if((int)m_qTags.size() >= m_nMaxSize)
					_AdjustQueue(idx);
			}
			return true;
		}
		++idx;
	}
	return false;
}

void CPriorityQueue::Clear()
{
	m_qTags.clear();
}

void CPriorityQueue::PushNode(const TAGRES &addTag)
{
	//cout << addTag.strTag.c_str() << " " << addTag.lfScore << endl;
	if( _IsExistinQueue(addTag))
	{
		return;
	}
	if((int)m_qTags.size() < m_nMaxSize)
    {
		m_qTags.push_back(addTag);
        if((int)m_qTags.size() == m_nMaxSize)
        {
			sort(m_qTags.begin(), m_qTags.end());
        }
    } else if(addTag.lfScore > m_qTags[0].lfScore) {
		//cout<<m_qTags[0].lfScore<<endl;
		m_qTags[0].strTag = addTag.strTag;
		m_qTags[0].lfScore = addTag.lfScore;
		m_qTags[0].vmodID = addTag.vmodID;
		m_qTags[0].lfFlankingMass[0] = addTag.lfFlankingMass[0];
		m_qTags[0].lfFlankingMass[1] = addTag.lfFlankingMass[1];

		_AdjustQueue( 0 );
   //     while (1)
   //     {
   //         int tPosLeftChild = (tPos << 1) + 1;
			//if (tPosLeftChild < m_nMaxSize && m_qTags[tPos].lfScore > m_qTags[tPosLeftChild].lfScore)
   //         { // lfScore of the tPos node is greater than its left child
   //             if (tPosLeftChild + 1 < m_nMaxSize && m_qTags[tPos].lfScore > m_qTags[tPosLeftChild + 1].lfScore)
   //             { // lfScore of the tPos node is greater than its right child
   //                 if (m_qTags[tPosLeftChild].lfScore < m_qTags[tPosLeftChild + 1].lfScore)
   //                 { // The lfScore of the left child is smaller
   //                     swap(m_qTags[tPos], m_qTags[tPosLeftChild]);
   //                     tPos = tPosLeftChild;
   //                 } else { // The lfScore of the right child is smaller
   //                     swap(m_qTags[tPos], m_qTags[tPosLeftChild + 1]);
   //                     tPos = tPosLeftChild + 1;
   //                 }
   //             } else { // lfScore of the tPos node is no greater than its right child
   //                 swap(m_qTags[tPos], m_qTags[tPosLeftChild]);
   //                 tPos = tPosLeftChild;
   //             }
   //         } else { // lfScore of the tPos node is no greater than its left child
   //              if (tPosLeftChild + 1 < m_nMaxSize && m_qTags[tPos].lfScore > m_qTags[tPosLeftChild + 1].lfScore)
   //              {
   //                    swap(m_qTags[tPos], m_qTags[tPosLeftChild + 1]);
   //                    tPos = tPosLeftChild + 1;
   //              } else break;
   //         }
   //     } // end while
		
    } // end else if
}

void CPriorityQueue::Print()
{
	vector<TAGRES>::iterator it;
	for(it = m_qTags.begin(); it != m_qTags.end(); ++it)
	{
		cout << it->strTag << ' ' << it->lfScore << ' ';
		cout << it->lfFlankingMass[0] << ' ' << it->lfFlankingMass[1] << ' ';
		for(int i = 0; i < (int)(it->vmodID.size()); ++i)
		{
			cout<<it->vmodID[i]<<' ';
		}
		cout << endl;
	}
}

int CPriorityQueue::GetTagSize()
{
	return m_qTags.size();
}

TAGRES& CPriorityQueue::GetTagItem(int idx)
{
	return m_qTags[idx];
}