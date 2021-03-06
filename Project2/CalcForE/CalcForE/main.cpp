#include <cmath>
#include <functional>
#include <iostream>
#include <iomanip>
#include <windows.h>
typedef long double _type;
inline void Test (std::function<_type (const int n)> f, const int n);
inline _type TaylorE (const int n);
inline _type DiffE (const int n);
inline _type IntegE (const int n);
int main (int argc, char** argv) {
	std::cout << std::fixed << std::setprecision (15);
	Test (TaylorE, 1e2);
	Test (DiffE, 1e4);
	Test (IntegE, 1e5);
	return 0;
}
inline void Test (std::function<_type (const int n)> f, const int n) {
	LARGE_INTEGER start_time, stop_time, freq;
	_type e;
	QueryPerformanceFrequency (&freq);
	QueryPerformanceCounter (&start_time);
	e = f (n);
	QueryPerformanceCounter (&stop_time);
	std::cout << e << "  Time spend: ";
	std::cout << 1000 * (double)(stop_time.QuadPart - start_time.QuadPart) / (double)freq.QuadPart;
	std::cout << " ms" << std::endl;
}
_type TaylorE (const int n) {
	_type eTaylor = 1.0 / (n + 1);
	for (int i = n; i; --i)
		++eTaylor /= i;
	return ++eTaylor;
}
_type DiffE (const int n) {
	_type h = 1.0 / n, k1, k2, k3, k4, ry = 0;
	for (int i = 0; i < n; i++) {
		k1 = ry * h / 6.0 + 1;
		k2 = ry * h / 6.0 + 1 + h * k1 / 2;
		k3 = ry * h / 6.0 + 1 + h * k2 / 2;
		k4 = ry * h / 6.0 + 1 + h * k3;
		ry += (k1 + 2 * k2 + 2 * k3 + k4);
	}
	return ry * h / 6.0 + 1;
}
_type IntegE (const int n) {
	_type interval = 1.0 / n, p_value = 0.0, n_value = 0.0, iter = 1.0;
	std::function<_type (const _type)> f = [](const _type t) -> _type {return 1.0 / t; };
	std::function<_type (void)> Cotos = [&](void) -> _type {
		return interval * (7 * f (iter) + 32 * f (iter + 0.25 * interval) + 12 * f (iter + 0.5 * interval) +
			32 * f (iter + 0.75 * interval) + 7 * f (iter + interval)) / 90;//Cotos
	};
	std::function<_type (void)> Simpson = [&](void) -> _type {
		return interval * (f (iter) + 4 * f (iter + 0.5 * interval) + f (iter + interval)) / 6;
	};
	std::function<_type (void)> T = [&](void) -> _type {
		return interval * (f (iter) + f (iter + interval)) / 2;
	};
	std::function<_type (void)> Intg_func = T;
	p_value = Intg_func ();
	while (p_value < 1.0) {
		n_value = p_value;
		iter += interval;
		p_value += Intg_func ();
	}
	return iter + interval * (1 - n_value) / (p_value - n_value);
}