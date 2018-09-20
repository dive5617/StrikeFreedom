/*
 *  TAGPriorityQueue.h
 *
 *  Created on: 2014-04-08
 *  Author: luolan
 */

#ifndef TAGPRIORTYQUEUE_H
#define TAGPRIORTYQUEUE_H

#include <iostream>
#include <cstring>
#include <vector>
#include <cmath>
#include "TagFlow.h"
#include "util.h"

using namespace std;

class CPriorityQueue{
private:
	int m_nMaxSize;
	vector<TAGRES> m_qTags;

	bool _IsExistinQueue(const TAGRES &addTag);
	void _AdjustQueue(int tPos);

	Clog *m_pTrace;

public:
    CPriorityQueue(const int maxsize);
    ~CPriorityQueue();
	void Clear();
	void Print();
    void PushNode(const TAGRES &addTag);
	int GetTagSize();
	void SortTagsBySeq();
	TAGRES& GetTagItem(int idx);
};

#endif