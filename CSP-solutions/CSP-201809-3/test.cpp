#include <string>
#include <iostream>
#include <map>
#include <sstream>
#include <algorithm>
#include <set>
#include <stack>
#include <vector>
using namespace std;

void split(const string &str, char delimiter, vector<string> &out)
{
	string last;
	for (int i = 0; i < str.size(); ++i)
		if (str[i] == delimiter)
		{
			out.push_back(last);
			last = "";
		}
		else
		{
			last += str[i];
		}
	out.push_back(last);
}

bool equalsIgnoreCase(const string &a, const string &b)
{
	if (a.size() != b.size())
		return false;
	for (int i = 0; i < a.size(); ++i)
		if (tolower(a[i]) != tolower(b[i]))
			return false;
	return true;
}

struct tree
{
	int id, dots;
	string tag, name;
	tree *parent;

	tree(int id, int dots, string tag, string name)
		: id(id), dots(dots), tag(tag), name(name), parent(0)
	{ }
};

bool apply(string selector, tree *t)
{
	if (selector[0] == '#') return selector == t->name;
	else return equalsIgnoreCase(selector, t->tag);
}

int main()
{
	int n, m;
	string line;
	cin >> n >> m;

	getline(cin, line);

	vector<tree *> nodes;
	stack<tree *> sk;

	for (int i = 1; i <= n; ++i)
	{
		getline(cin, line);

		int dots = 0;
		while (line[dots] == '.') dots++;

		string tag, id;
		stringstream ss(line.substr(dots));
		ss >> tag >> id;

		tree *now = new tree(i, dots, tag, id);
		if (!sk.empty())
		{
			tree *top;
			while (top = sk.top(), dots <= top->dots)
				sk.pop();

			now->parent = top;
		}
		sk.push(now);

		nodes.push_back(now);
	}

	while (m--)
	{
		getline(cin, line);
		vector<string> selector;
		split(line, ' ', selector);

		vector<int> ans;

		set<tree *> s;
		for (int i = 0; i < nodes.size(); ++i)
		{
			if (apply(selector.back(), nodes[i]))
				s.insert(nodes[i]);
		}

		for (set<tree *>::iterator it = s.begin(); it != s.end(); ++it)
		{
			tree *t = *it;
			int sel = selector.size() - 1;
			while (t && sel >= 0)
			{
				if (apply(selector[sel], t))
					--sel;
				t = t->parent;
			}
			if (sel == -1)
				ans.push_back((*it)->id);
		}

		cout << ans.size() << ' ';
		sort(ans.begin(), ans.end());
		for (int i = 0; i < ans.size(); ++i)
			cout << ans[i] << ' ';
		cout << endl;
	}

	return 0;
}