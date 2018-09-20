/*
 *  test.h
 *
 *  Created on: 2016-06-08
 *  Author: wrm
 */

#ifndef _TEST_H_
#define _TEST_H_

#include <iostream>
#include <algorithm>
#include <unordered_set>
#include "sdk.h"
#include "searcher.h"
#include "index.h"
using namespace std;

class Test{
public:
	static void testFilter(string configfile);
	static void testRerank(string configfile);

};


#endif
